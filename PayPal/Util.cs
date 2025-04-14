using System.Globalization;

namespace CloudSubscription.PayPal
{
    public class Util
    {
        /// <summary>
        /// Generates a PayPal payment link for a single purchase.
        /// </summary>
        /// <param name="businessEmail">The PayPal account email address to receive the payment.</param>
        /// <param name="productName">The name or description of the product being purchased.</param>
        /// <param name="amount">The payment amount, formatted to two decimal places.</param>
        /// <param name="currency">The currency code for the payment (e.g., "EUR" for Euros).</param>
        /// <param name="purchaseId">A unique purchase ID for transaction tracking.</param>
        /// <param name="returnUrl">An optional URL to redirect the user after a successful payment.</param>
        /// <param name="cancelUrl">An optional URL to redirect the user if the payment is canceled.</param>
        /// <returns>
        /// A string representing the full PayPal payment link.
        /// </returns>
        static public string GeneratePayPalLink(string businessEmail, string productName, double amount, string currency, string purchaseId, string returnUrl = "", string cancelUrl = "")
        {
            // Base URL for PayPal payments
            string baseUrl = "https://www.paypal.com/cgi-bin/webscr";

            // Mandatory parameters for the PayPal payment link
            var parameters = new Dictionary<string, string>
                {
                    { "cmd", "_xclick" }, // Indicates the type of PayPal operation (single item purchase)
                    { "business", businessEmail }, // Email of the PayPal account receiving the payment
                    { "item_name", productName }, // Name or description of the item being purchased
                    { "amount", amount.ToString("F2", CultureInfo.InvariantCulture) }, // Payment amount formatted with two decimal places
                    { "currency_code", currency }, // Currency code for the payment (e.g., "EUR" for Euros)
                    { "custom", purchaseId } // Custom field to store a unique purchase ID for transaction tracking
                };

            // Add optional parameters if the URLs for return and cancellation are provided
            if (!string.IsNullOrEmpty(returnUrl))
            {
                parameters.Add("return", returnUrl); // URL to redirect the user after successful payment
            }

            if (!string.IsNullOrEmpty(cancelUrl))
            {
                parameters.Add("cancel_return", cancelUrl); // URL to redirect the user if they cancel the payment
            }

            // Build the query string from the dictionary of parameters
            var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

            // Return the complete PayPal payment link
            return $"{baseUrl}?{queryString}";
        }


    }
}
