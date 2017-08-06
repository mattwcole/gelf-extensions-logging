using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public class GelfLogger : ILogger
    {
        private readonly string _name;
        private readonly GelfMessageProcessor _messageProcessor;
        private readonly GelfLoggerOptions _options;

        public GelfLogger(string name, GelfMessageProcessor messageProcessor, GelfLoggerOptions options)
        {
            _name = name;
            _messageProcessor = messageProcessor;
            _options = options;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.None || !IsEnabled(logLevel))
            {
                return;
            }

            var additionalFields = new Dictionary<string, string>(2)
            {
                ["logger"] = _name
            };
            if (exception != null)
            {
                additionalFields["exception"] = exception.ToString();
            }

            var message = new GelfMessage
            {
                ShortMessage = formatter(state, exception),
                Host = _options.LogSource,
                Level = GetLevel(logLevel),
                Timestamp = GetTimestamp(),
                AdditionalFields = additionalFields.Concat(_options.AdditionalFields)
                    .Concat(GetScopeAdditionalFields())
            };

            _messageProcessor.SendMessage(message);
        }

        private static ICollection<KeyValuePair<string, string>> GetScopeAdditionalFields()
        {
            var additionalFields = Enumerable.Empty<KeyValuePair<string, string>>();

            var scope = GelfLogScope.Current;
            while (scope != null)
            {
                additionalFields = additionalFields.Concat(scope.AdditionalFields);
                scope = scope.Parent;
            }

            return additionalFields.ToArray();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _options.Filter == null || _options.Filter(_name, logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var additionalField = state as ValueTuple<string, string>?;
            if (additionalField.HasValue)
            {
                var field = additionalField.Value;
                return GelfLogScope.Push(new[]
                {
                    new KeyValuePair<string, string>(field.Item1, field.Item2)
                });
            }

            if (state is IEnumerable<KeyValuePair<string, string>> additionalFields)
            {
                return GelfLogScope.Push(additionalFields);
            }

            return new NoopDisposable();
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
