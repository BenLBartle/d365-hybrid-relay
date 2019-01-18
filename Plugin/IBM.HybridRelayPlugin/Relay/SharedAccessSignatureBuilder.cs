using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace IBM.HybridRelayPlugin.Relay
{
    public static class SharedAccessSignatureBuilder
    {
        public static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static string BuildSignature(
            string keyName,
            byte[] encodedSharedAccessKey,
            string resource,
            TimeSpan timeToLive)
        {
            // Note that target URI is not normalized because in IoT scenario it 
            // is case sensitive.
            string expiresOn = BuildExpiresOn(timeToLive);
            string audienceUri = WebUtility.UrlEncode(resource);
            List<string> fields = new List<string> { audienceUri, expiresOn };

            // Example string to be signed:
            // http://mynamespace.servicebus.windows.net/a/b/c?myvalue1=a
            // <Value for ExpiresOn>
            string signature = Sign(string.Join("\n", fields), encodedSharedAccessKey);

            // Example returned string:
            // SharedAccessKeySignature
            // sr=ENCODED(http://mynamespace.servicebus.windows.net/a/b/c?myvalue1=a)&sig=<Signature>&se=<ExpiresOnValue>&skn=<KeyName>

            return string.Format(CultureInfo.InvariantCulture, "{0} {1}={2}&{3}={4}&{5}={6}&{7}={8}",
                SharedAccessSignatureToken.SharedAccessSignature,
                SharedAccessSignatureToken.SignedResource, audienceUri,
                SharedAccessSignatureToken.Signature, WebUtility.UrlEncode(signature),
                SharedAccessSignatureToken.SignedExpiry, WebUtility.UrlEncode(expiresOn),
                SharedAccessSignatureToken.SignedKeyName, WebUtility.UrlEncode(keyName));
        }

        static string BuildExpiresOn(TimeSpan timeToLive)
        {
            DateTime expiresOn = DateTime.UtcNow.Add(timeToLive);
            TimeSpan secondsFromBaseTime = expiresOn.Subtract(EpochTime);
            long seconds = Convert.ToInt64(secondsFromBaseTime.TotalSeconds, CultureInfo.InvariantCulture);
            return Convert.ToString(seconds, CultureInfo.InvariantCulture);
        }

        static string Sign(string requestString, byte[] encodedSharedAccessKey)
        {
            using (var hmac = new HMACSHA256(encodedSharedAccessKey))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(requestString)));
            }
        }
    }
}
