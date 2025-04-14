using System.Text;
namespace CloudSubscription
{
    static internal class Events
    {
        static internal void OnPaymentCompleted(string idHex)
        {
            var subscription = Panels.CreateNewSubscription.Load(idHex, out string jsonString);
            using var client = new HttpClient();
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.PostAsync(Settings.ApiEndpoint, content); // Send POST request   
        }
    }
}
