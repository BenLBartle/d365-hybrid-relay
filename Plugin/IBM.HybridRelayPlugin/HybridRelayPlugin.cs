namespace IBM.HybridRelayPlugin
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using IBM.HybridRelayPlugin.Config;
    using IBM.HybridRelayPlugin.MessageHandlers;
    using Microsoft.Xrm.Sdk;

    public class HybridRelayPlugin : IPlugin
    {
        private readonly string _secureConfiguration;
        private readonly string _unsecureConfiguration;

        public HybridRelayPlugin(string unsecureConfiguration, string secureConfiguration)
        {
            if (string.IsNullOrWhiteSpace(secureConfiguration))
            {
                throw new InvalidPluginExecutionException("Secure configuriation is required for this plugin to execute.");
            }
            _secureConfiguration = secureConfiguration;
            _unsecureConfiguration = unsecureConfiguration;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                tracer.Trace("Parsing Input");
                Entity entity = (Entity)context.InputParameters["Target"];

                tracer.Trace("Parsing Relay Configuration");
                RelayConfiguration config = GetConfig();

                tracer.Trace("Creating HttpClient");
                using (var client = new HttpClient(new HybridRelayHandler(config)))
                {
                    tracer.Trace("Executing Request");
                    var response = client.GetStringAsync($"{config.Url}/api/values").Result;

                    tracer.Trace("Updating Target");
                    entity["tickersymbol"] = response;
                }
            }
        }

        private RelayConfiguration GetConfig()
        {
            RelayConfiguration config;
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(_secureConfiguration)))
            {
                var deserializer = new DataContractJsonSerializer(typeof(RelayConfiguration));
                config = (RelayConfiguration)deserializer.ReadObject(ms);
            }

            return config;
        }
    }
}
