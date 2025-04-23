using System.Diagnostics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;

namespace CloudSubscription
{
    static internal class Events
    {
        static internal void OnPaymentCompleted(Dictionary<string, string> instantPaymentNotificationData)
        {
            // If the payment is completed, extract transaction details
            var transactionId = instantPaymentNotificationData["txn_id"]; // Transaction ID from PayPal
            var idHex = instantPaymentNotificationData["custom"]; // Custom field set in the payment link
            var amount = instantPaymentNotificationData["mc_gross"]; // Gross amount of the transaction
            Debug.WriteLine($"Payment completed. Transaction ID: {transactionId}, Custom ID: {idHex}, Amount: {amount}");

            var subscription = Panels.CreateNewSubscription.Load(Panels.CreateNewSubscription.Step.Pending, idHex);
            subscription.ServiceExpires = DateTime.UtcNow.AddDays(subscription.DurationOfSubscriptionInDays);
            subscription.Save();
            bool success = false;
            int maxRetries = 48; // Maximum number of retry attempts
            int attempts = 0;

            Timer? retryTimer = null; // Declare the timer

            void AttemptApiCall(object? state)
            {
                if (attempts >= maxRetries || success)
                {
                    retryTimer?.Dispose(); // Stop and clean up the timer if finished
                    return;
                }
                attempts++;
                try
                {
                    using var client = new HttpClient();
                    var content = new StringContent(subscription.JsonString, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(Settings.ApiEndpoint, content).Result; // Send POST request
                    string responseContent = response.Content.ReadAsStringAsync().Result;

                    // Parse the JSON response and extract the "result" field
                    var jsonDocument = JsonDocument.Parse(responseContent);
                    if (jsonDocument.RootElement.TryGetProperty("CloudId", out var resultElementCloudId) && jsonDocument.RootElement.TryGetProperty("QrEncrypted", out var resultElementQrEncrypted))
                    {
                        // Extract "CloudId" as ulong
                        subscription.CloudId = resultElementCloudId.GetUInt64();

                        // Extract "QrEncrypted" as string
                        subscription.QrEncrypted = resultElementQrEncrypted.GetString();
                        EncryptedKeyIdHexDictionary[idHex] = subscription.QrEncrypted;

                        subscription.Save();

                        success = true;
                        retryTimer?.Dispose(); // Stop and clean up the timer after success
                    }
                    else
                    {
                        Console.WriteLine("Invalid or missing 'result' field in response: " + responseContent);
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    Console.WriteLine($"Attempt failed ({attempts}/{maxRetries}): {ex.Message}");
                }
            }

            // Create and start the timer with an interval of 30 minutes
            retryTimer = new Timer(AttemptApiCall, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
        }

        internal static Dictionary<string, string> EncryptedKeyIdHexDictionary = [];
    }
}
