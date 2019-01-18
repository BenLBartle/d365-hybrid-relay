// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IBM.HybridRelayPlugin.Relay
{
    using Microsoft.Xrm.Sdk;
    using System;

    static class UriHelper
    {
        public static Uri NormalizeUri(string uri, string scheme, bool stripQueryParameters = true, bool stripPath = false, bool ensureTrailingSlash = false)
        {
            UriBuilder uriBuilder = new UriBuilder(uri)
            {
                Scheme = scheme,
                Port = -1,
                Fragment = string.Empty,
                Password = string.Empty,
                UserName = string.Empty,
            };

            if (stripPath)
            {
                uriBuilder.Path = string.Empty;
            }

            if (stripQueryParameters)
            {
                uriBuilder.Query = string.Empty;
            }

            if (ensureTrailingSlash)
            {
                if (!uriBuilder.Path.EndsWith("/", StringComparison.Ordinal))
                {
                    uriBuilder.Path += "/";
                }
            }

            return uriBuilder.Uri;
        }

        public static void ThrowIfNullAddressOrPathExists(Uri address, string paramName, IOrganizationService service)
        {
            if (address == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (!string.IsNullOrEmpty(address.AbsolutePath) && address.Segments.Length > 1)
            {
               throw new ArgumentException();
            }
        }
    }
}