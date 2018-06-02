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
        private readonly IDisposable _gelfClient;


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

            IGelfClient gelfClient = null;

            if (options.Host.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) || options.Host.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
            {
                gelfClient = new HttpGelfClient(options);
            }
            else
            {
                gelfClient = new UdpGelfClient(options);
            }

            _options = options;
            _gelfClient = gelfClient;
            _messageProcessor = new GelfMessageProcessor(gelfClient);
            _messageProcessor.Start();
        }

        public ILogger CreateLogger(string name)
        {
            return new GelfLogger(name, _messageProcessor, _options);
        }

        public void Dispose()
        {
            _messageProcessor.Stop();
            _gelfClient.Dispose();
        }
    }
}
