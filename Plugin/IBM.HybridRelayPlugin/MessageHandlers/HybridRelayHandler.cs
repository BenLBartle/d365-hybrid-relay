namespace IBM.HybridRelayPlugin.MessageHandlers
{
    using System;
    using System.Net.Http;
    using System.Runtime.Caching;
    using System.Threading;
    using System.Threading.Tasks;
    using IBM.HybridRelayPlugin.Config;
    using IBM.HybridRelayPlugin.Relay;

    public class HybridRelayHandler : DelegatingHandler
    {
        private readonly RelayConfiguration _config;
        private readonly SharedAccessSignatureTokenProvider _tokenProvider;

        private readonly TimeSpan _timespan = TimeSpan.FromMinutes(2);

        private readonly MemoryCache _cache = new MemoryCache("TokenCache");
        private static readonly object _cacheLock = new object();

        public HybridRelayHandler(RelayConfiguration relayConfig) : base()
        {
            _config = relayConfig;
            _tokenProvider = new SharedAccessSignatureTokenProvider(_config.SasKeyName, _config.SasKey);
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("ServiceBusAuthorization", GenerateToken(_timespan));
            return await base.SendAsync(request, cancellationToken);
        }

        private string GenerateToken(TimeSpan timeSpan)
        {
            SharedAccessSignatureToken token;

            lock (_cacheLock)
            {
                token = (SharedAccessSignatureToken)_cache.Get("relaytoken");

                if (token == null)
                {
                    token = _tokenProvider.GetTokenAsync(_config.Url, timeSpan).Result;
                    _cache.Add("relaytoken", token, new DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), timeSpan));
                }
            }

            return token?.TokenString;
        }
    }
}
