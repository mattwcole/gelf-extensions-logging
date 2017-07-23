using System;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public class GelfLogger : ILogger
    {
        private readonly GelfMessageProcessor _messageProcessor;
        private readonly GelfLoggerOptions _options;

        public GelfLogger(GelfMessageProcessor messageProcessor, GelfLoggerOptions options)
        {
            _messageProcessor = messageProcessor;
            _options = options;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = new GelfMessage
            {
                ShortMessage = formatter(state, exception),
                Host = _options.AppHost,
                Level = GetLevel(logLevel),
                Timestamp = GetTimestamp()
            };

            _messageProcessor.SendMessage(message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;    // TODO
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();    // TODO
        }

        private static SyslogSeverity GetLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return SyslogSeverity.Debug;
                case LogLevel.Information:
                    return SyslogSeverity.Informational;
                case LogLevel.Warning:
                    return SyslogSeverity.Warning;
                case LogLevel.Error:
                    return SyslogSeverity.Error;
                case LogLevel.Critical:
                    return SyslogSeverity.Critical;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Log level not supported.");
            }
        }

        private static string GetTimestamp()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var totalSeconds = utcNow.ToUnixTimeSeconds();
            var totalMiliseconds = utcNow.ToUnixTimeMilliseconds();

            return $"{totalSeconds}.{totalMiliseconds % 1000}";
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
