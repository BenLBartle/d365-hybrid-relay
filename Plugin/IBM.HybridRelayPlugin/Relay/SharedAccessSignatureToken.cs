namespace IBM.HybridRelayPlugin.Relay
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.Net;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// A WCF SecurityToken that wraps a Shared Access Signature
    /// </summary>
    public class SharedAccessSignatureToken : SecurityToken
    {
        public const int MaxKeyNameLength = 256;
        public const int MaxKeyLength = 256;
        public const string SharedAccessSignature = "SharedAccessSignature";
        public const string SignedResource = "sr";
        public const string Signature = "sig";
        public const string SignedKeyName = "skn";
        public const string SignedExpiry = "se";
        public const string SignedResourceFullFieldName = SharedAccessSignature + " " + SignedResource;
        public const string SasKeyValueSeparator = "=";
        public const string SasPairSeparator = "&";
        public string TokenString { get; }
        internal IOrganizationService service;

        public SharedAccessSignatureToken(string tokenString, IOrganizationService orgService)
        {
            TokenString = tokenString;
            service = orgService;
        }

        public override string Id => throw new NotImplementedException();

        public override ReadOnlyCollection<SecurityKey> SecurityKeys => throw new NotImplementedException();

        public override DateTime ValidFrom => throw new NotImplementedException();

        public override DateTime ValidTo => throw new NotImplementedException();

        internal static void Validate(string sharedAccessSignature, IOrganizationService service)
        {
            if (string.IsNullOrEmpty(sharedAccessSignature))
            {
                throw new ArgumentNullException();
            }

            IDictionary<string, string> parsedFields = ExtractFieldValues(sharedAccessSignature);

            if (!parsedFields.TryGetValue(Signature, out string signature))
            {
                throw new ArgumentNullException(nameof(Signature));
            }

            if (!parsedFields.TryGetValue(SignedExpiry, out string expiry))
            {
                throw new ArgumentNullException(nameof(SignedExpiry));
            }

            if (!parsedFields.TryGetValue(SignedKeyName, out string keyName))
            {
                throw new ArgumentNullException(nameof(SignedKeyName));
            }

            if (!parsedFields.TryGetValue(SignedResource, out string encodedAudience))
            {
                throw new ArgumentNullException(nameof(SignedResource));
            }
        }

        static IDictionary<string, string> ExtractFieldValues(string sharedAccessSignature)
        {
            string[] tokenLines = sharedAccessSignature.Split();

            if (!string.Equals(tokenLines[0].Trim(), SharedAccessSignature, StringComparison.OrdinalIgnoreCase) || tokenLines.Length != 2)
            {
                new ArgumentNullException(nameof(sharedAccessSignature));
            }

            IDictionary<string, string> parsedFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] tokenFields = tokenLines[1].Trim().Split(new string[] { SasPairSeparator }, StringSplitOptions.None);

            foreach (string tokenField in tokenFields)
            {
                if (tokenField != string.Empty)
                {
                    string[] fieldParts = tokenField.Split(new string[] { SasKeyValueSeparator }, StringSplitOptions.None);
                    if (string.Equals(fieldParts[0], SignedResource, StringComparison.OrdinalIgnoreCase))
                    {
                        // We need to preserve the casing of the escape characters in the audience,
                        // so defer decoding the URL until later.
                        parsedFields.Add(fieldParts[0], fieldParts[1]);
                    }
                    else
                    {
                        parsedFields.Add(fieldParts[0], WebUtility.UrlDecode(fieldParts[1]));
                    }
                }
            }

            return parsedFields;
        }
    }
}
