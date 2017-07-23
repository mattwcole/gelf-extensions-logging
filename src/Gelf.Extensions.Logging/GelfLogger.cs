using System;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public class GelfLogger : ILogger
    {
        private readonly GelfMessageProcessor _messageProcessor;

        public GelfLogger(GelfMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = new GelfMessage
            {
                ShortMessage = formatter(state, exception)
            };

            _messageProcessor.SendMessage(message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
