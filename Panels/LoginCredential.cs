using System.Diagnostics;

namespace CloudSubscription.Panels
{
    /// <summary>
    /// Cloud service login credentials (Keep the encrypted QR code and do not lose it. It will be entered to connect the client to the cloud)
    /// </summary>
    public class LoginCredential
    {
        internal bool Hidden = true;

        /// <summary>
        /// IDHex Assigned by query string
        /// </summary>
        [AsQueryString]
        internal string? IdHex;

        /// <summary>
        /// Keep this code and do not lose it. Together with the 2FA that was sent to you by email, it represents the credentials for accessing the cloud service.
        /// </summary>
        public string? QrEncrypted
        {
            get
            {
                string? qrEncrypted = null;
                int timeoutMilliseconds = 128000;
                int elapsedMilliseconds = 0;
                int checkInterval = 1000;
                if (IdHex == null)
                {
                    throw new ArgumentNullException(nameof(IdHex), "IdHex cannot be null.");
                }
                while (qrEncrypted == null && elapsedMilliseconds < timeoutMilliseconds)
                {
                    if (Events.EncryptedKeyIdHexDictionary.ContainsKey(IdHex))
                    {
                        qrEncrypted = Events.EncryptedKeyIdHexDictionary[IdHex];
                    }
                    else
                    {
                        Thread.Sleep(checkInterval);
                        elapsedMilliseconds += checkInterval;
                    }
                }
                if (qrEncrypted == null)
                {
                    throw new TimeoutException("Timeout while waiting for EncrypredQrCode to be set.");
                }
                return qrEncrypted;
            }
        }
    }
}
