// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IBM.HybridRelayPlugin.Relay
{
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The SharedAccessSignatureTokenProvider generates tokens using a shared access key or existing signature.
    /// </summary>
    public class SharedAccessSignatureTokenProvider : TokenProvider
    {
        private readonly byte[] _encodedSharedAccessKey;
        private readonly string _keyName;
        private readonly string _sharedAccessSignature;
        static IOrganizationService OrganizationService { get; set; }

        public SharedAccessSignatureTokenProvider(string keyName, string sharedAccessKey)
            : this(keyName, sharedAccessKey, MessagingTokenProviderKeyEncoder, OrganizationService)
        {
        }

        protected SharedAccessSignatureTokenProvider(string keyName, string sharedAccessKey, Func<string, byte[]> customKeyEncoder, IOrganizationService orgService)
        {

            OrganizationService = orgService;

            if (string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(sharedAccessKey))
            {
                throw new ArgumentNullException(nameof(keyName));
            }

            if (keyName.Length > SharedAccessSignatureToken.MaxKeyNameLength)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (sharedAccessKey.Length > SharedAccessSignatureToken.MaxKeyLength)
            {
                throw new ArgumentOutOfRangeException();
            }

            _keyName = keyName;
            _encodedSharedAccessKey = customKeyEncoder != null ?
                customKeyEncoder(sharedAccessKey) :
                MessagingTokenProviderKeyEncoder(sharedAccessKey);
        }

        protected override Task<SharedAccessSignatureToken> OnGetTokenAsync(string resource, TimeSpan validFor)
        {
            string tokenString = this.BuildSignature(resource, validFor);
            var securityToken = new SharedAccessSignatureToken(tokenString, OrganizationService);
            return Task.FromResult(securityToken);
        }

        protected virtual string BuildSignature(string resource, TimeSpan validFor)
        {
            if (string.IsNullOrWhiteSpace(_sharedAccessSignature))
            {
                return SharedAccessSignatureBuilder.BuildSignature(_keyName, _encodedSharedAccessKey, resource, validFor);
            }

            return _sharedAccessSignature;
        }

        
    }

    
}