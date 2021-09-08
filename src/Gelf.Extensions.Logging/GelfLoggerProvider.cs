using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gelf.Extensions.Logging
{
    [ProviderAlias("GELF")]
    public class GelfLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly GelfLoggerOptions _options;
        private readonly GelfMessageProcessor _messageProcessor;
        private readonly IGelfClient _gelfClient;
        private readonly ConcurrentDictionary<string, GelfLogger> _loggers;

        private IExternalScopeProvider? _scopeProvider;

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
            _loggers = new ConcurrentDictionary<string, GelfLogger>();
        }

        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, newName => new GelfLogger(newName, _messageProcessor, _options)
            {
                ScopeProvider = _scopeProvider
            });
        }

        private static IGelfClient CreateGelfClient(GelfLoggerOptions options)
        {
            return options.Protocol switch
            {
                GelfProtocol.Udp => new UdpGelfClient(options),
                GelfProtocol.Http => new HttpGelfClient(options),
                GelfProtocol.Https => new HttpGelfClient(options),
                _ => throw new ArgumentException("Unknown protocol.", nameof(options))
            };
        }

        public void Dispose()
        {
            _messageProcessor.Stop();
            _gelfClient.Dispose();
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
            foreach (var logger in _loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }
        }
    }
}
