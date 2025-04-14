using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
namespace CloudSubscription.Panels
{
    public class CreateNewSubscription
    {
        /// <summary>
        /// This element is only useful to insert as a json parameter, it will be hidden from the GUI thanks to the attribute: [HiddenFromGUI]
        /// You can use [HiddenFromGUI] to hide public properties that should not be displayed in the GUI.
        /// </summary>
        [HiddenFromGUI]
        public string Method => nameof(CreateNewSubscription);

        /// <summary>
        /// Cost base in Euro (Daily cost per GB before discount)
        /// </summary>
        public double BaseDailyCostForGb { get; } = 0.005;

        /// <summary>
        /// Effective daily cost per GB with volume discount
        /// </summary>
        public double EffectiveDailyCostForGb => Math.Round(CostInEuro / (StorageSpaceGb * DurationOfSubscriptionInDays), 4);

        /// <summary>
        /// Gigabytes of space required
        /// </summary>
        public int StorageSpaceGb { get; set; } = 16;

        internal int StorageSpaceGb_Min = 16;

        internal int StorageSpaceGb_Max = 4096;

        internal int StorageSpaceGb_Step = 16;

        /// <summary>
        /// Subscription duration in days
        /// </summary>
        public int DurationOfSubscriptionInDays { get; set; } = 30;

        internal int DurationOfSubscriptionInDays_Min = 30;

        internal int DurationOfSubscriptionInDays_Max = 365;

        private double Coefficient => StorageSpaceGb * DurationOfSubscriptionInDays;

        /// <summary>
        /// Coefficient discount
        /// </summary>
        private double CoefficientDiscount => DiscountCoeficent(Coefficient);

        /// <summary>
        /// Full cost not discounted
        /// </summary>
        public double FullCostInEuro => Math.Round(BaseDailyCostForGb * Coefficient, 2);


        /// <summary>
        /// Automatic discount that increases as the subscription features increase
        /// </summary>
        public string DiscountApplied => "Discount of €" + Math.Round(FullCostInEuro - CostInEuro, 2) + " (~" + Math.Round((1 - CoefficientDiscount) * 100, 0) + "%)";


        /// <summary>
        /// Total cost in Euro, discounted
        /// </summary>
        public double CostInEuro => Math.Round(FullCostInEuro * CoefficientDiscount, 2);

        static double DiscountCoeficent(double x)
        {
            return 0.2 + 0.8 * Math.Exp(-0.00001 * x);
        }

        /// <summary>
        /// Subscriber's email address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Redirect the user to specific web address;
        /// </summary>
        internal Uri? Redirect = null;

        /// <summary>
        /// Confirm your subscription to the cloud service, as per your settings
        /// </summary>
        /// <returns></returns>
        [DebuggerHidden]
        public string Submit()
        {
            if (Email == null || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) // validate Email address
            {
                throw new ArgumentException("Invalid email address!");
            }
            string jsonString = JsonSerializer.Serialize(this);
            var id = BitConverter.ToUInt64([.. SHA256.HashData(Encoding.UTF8.GetBytes(jsonString)).Take(8)]);
            var idHex = id.ToString("X");
            File.WriteAllText(Path.Combine(DataPath.FullName, idHex + ".subscription"), jsonString);
            var paypalLink = PayPal.Util.GeneratePayPalLink(Settings.PayPalBusinessEmail, "Cloud Storage, Day=" + DurationOfSubscriptionInDays + ", GB=" + StorageSpaceGb, CostInEuro, "EUR", idHex);
            Redirect = new Uri(paypalLink);
            return "Redirection to the payment platform";
        }

        internal static CreateNewSubscription Load(string idHex, out string jsonString)
        {
            var filePath = Path.Combine(DataPath.FullName, idHex + ".subscription");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file with ID '{idHex}' does not exist.");
            }
            jsonString = File.ReadAllText(filePath);
            var subscription = JsonSerializer.Deserialize<CreateNewSubscription>(jsonString);
            if (subscription == null)
            {
                throw new InvalidOperationException("Failed to load subscription. The data might be corrupted.");
            }
            return subscription;
        }

        static private DirectoryInfo DataPath
        {
            get
            {
                var dataPath = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "AppData"));
                dataPath.Create(); // Create the directory if it doesn't exist
                return dataPath;
            }
        }

    }
}
