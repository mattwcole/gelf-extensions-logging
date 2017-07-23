using System;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public class GelfLoggerProvider : ILoggerProvider
    {
        private readonly GelfMessageProcessor _messageProcessor;
        private readonly IDisposable _gelfClient;

        public GelfLoggerProvider(GelfLoggerOptions options)
        {
            var gelfClient = new UdpGelfClient(options);

            _gelfClient = gelfClient;
            _messageProcessor = new GelfMessageProcessor(gelfClient);
            _messageProcessor.Start();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new GelfLogger(_messageProcessor);
        }

        public void Dispose()
        {
            _messageProcessor.Dispose();
            _gelfClient.Dispose();
        }
    }
}
