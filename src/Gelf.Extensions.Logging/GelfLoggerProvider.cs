using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gelf.Extensions.Logging
{
#if NETSTANDARD2_0
    [ProviderAlias("GELF")]
#endif
    public class GelfLoggerProvider : ILoggerProvider
    {
        private readonly GelfLoggerOptions _options;
        private readonly GelfMessageProcessor _messageProcessor;
        private readonly IGelfClient _gelfClient;
        
        public GelfLoggerProvider(IOptions<GelfLoggerOptions> options) : this(options.Value)
        {
        }

        public GelfLoggerProvider(GelfLoggerOptions options)
        {
            if (string.IsNullOrEmpty(options.Host))
            {
                throw new ArgumentException("GELF host is required.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.LogSource))
            {
                throw new ArgumentException("GELF log source is required.", nameof(options));
            }

            _options = options;
            _gelfClient = CreateGelfClient(_options);
            _messageProcessor = new GelfMessageProcessor(_gelfClient);
            _messageProcessor.Start();
        }

        public ILogger CreateLogger(string name)
        {
            return new GelfLogger(name, _messageProcessor, _options);
        }

        private static IGelfClient CreateGelfClient(GelfLoggerOptions options)
        {
            switch (options.Protocol)
            {
                case GelfProtocol.Udp:
                    return new UdpGelfClient(options);
                case GelfProtocol.Http:
                case GelfProtocol.Https:
                    return new HttpGelfClient(options);
                default:
                    throw new ArgumentException("Unknown protocol.", nameof(options));
            }
        }

        public void Dispose()
        {
            _messageProcessor.Stop();
            _gelfClient.Dispose();
        }
    }
}
