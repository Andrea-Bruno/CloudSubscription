using System.Text.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
        /// Id of subscription (hex value)
        /// </summary>
        [HiddenFromGUI]
        public string? IdHex { get; set; }

        /// <summary>
        /// Id of cloud instance on the server
        /// </summary>
        [HiddenFromGUI]
        public ulong CloudId { get; set; }


        /// <summary>
        /// The encrypted QR code string for connection
        /// </summary>
        [HiddenFromGUI]
        public string? QrEncrypted { get; set; }

        /// <summary>
        /// End of service(as it updates with payments)
        /// </summary>
        [HiddenFromGUI]
        public DateTime ServiceExpires { get; set; }

        /// <summary>
        /// Confirm your subscription to the cloud service, as per your settings.
        /// Once the payment is completed, press the "return to the seller's site" button, you will then see your encrypted QR (you will have to keep it, it is part of the cloud access credentials). The 2FA will instead be sent separately via email.
        /// </summary>
        /// <returns></returns>
        [DebuggerHidden]
        public string Submit()
        {
            if (Email == null || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) // validate Email address
            {
                throw new ArgumentException("Invalid email address!");
            }
            Save(Step.Pending);
            var session = UISupportBlazor.Session.Current();
            var LoginCredential = session.GetPanel(typeof(LoginCredential)) as LoginCredential;
            LoginCredential.Hidden = false;
            var baseUrl = session.Values["BaseUrl"] as string;
            var returnUrl = baseUrl + "/nav/" + typeof(LoginCredential).GUID + "?" + nameof(IdHex) + "=" + IdHex;
            var cancelUrl = baseUrl + "/cancel/";
            var info = "Cloud Storage, Day=" + DurationOfSubscriptionInDays + ", GB=" + StorageSpaceGb + ", " + nameof(IdHex) + "=" + IdHex;
            var paypalLink = PayPal.Util.GeneratePayPalLink(Settings.PayPalBusinessEmail, info, CostInEuro, "EUR", IdHex, true, returnUrl, cancelUrl, DurationOfSubscriptionInDays);
            Redirect = new Uri(paypalLink);
            return "Redirection to the payment platform";
        }

        private static string FileName(Step step, string idHex) => Path.Combine(DataPath.FullName, idHex + "." + step.ToString());

        internal void Save(Step step = Step.Subscription)
        {
            if (string.IsNullOrEmpty(IdHex))
            {
                var id = BitConverter.ToUInt64([.. Guid.NewGuid().ToByteArray().Take(8)]);
                IdHex = id.ToString("X");
            }
            if (step == Step.Subscription)
            {
                var pendingFile = FileName(Step.Pending, IdHex);
                if (File.Exists(pendingFile))
                    File.Delete(pendingFile);
            }

            File.WriteAllText(FileName(step, IdHex), JsonString);
        }

        internal string JsonString => JsonSerializer.Serialize(this);

        internal static CreateNewSubscription Load(Step step, string idHex)
        {
            var filePath = FileName(step, idHex);
            if (!File.Exists(filePath) && step == Step.Pending)
            {
                filePath = FileName(Step.Subscription, idHex);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"The file with ID '{idHex}' does not exist.");
                }
            }
            var jsonString = File.ReadAllText(filePath);
            var subscription = JsonSerializer.Deserialize<CreateNewSubscription>(jsonString);
            if (subscription == null)
            {
                throw new InvalidOperationException("Failed to load subscription. The data might be corrupted.");
            }
            return subscription;
        }

        internal enum Step
        {
            Pending,
            Subscription,
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
