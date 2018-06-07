using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;

namespace Gelf.Extensions.Logging
{
    public class GelfLogger : ILogger
    {
        private static readonly Regex AdditionalFieldKeyRegex = new Regex(@"^[\w\.\-]*$");
        private static readonly HashSet<string> ReservedAdditionalFieldKeys = new HashSet<string>
        {
            "id",
            "logger",
            "exception",
            "event_id",
            "event_name"
        };

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
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var additionalFields = _options.AdditionalFields
                .Concat(GetStateAdditionalFields(state))
                .Concat(GetScopeAdditionalFields());

            var message = new GelfMessage
            {
                ShortMessage = formatter(state, exception),
                Host = _options.LogSource,
                Level = GetLevel(logLevel),
                Timestamp = GetTimestamp(),
                Logger = _name,
                Exception = exception?.ToString(),
                EventId = eventId.Id,
                EventName = eventId.Name,
                AdditionalFields = ValidateAdditionalFields(additionalFields)
            };

            _messageProcessor.SendMessage(message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return logLevel != LogLevel.None && (_options.Filter == null || _options.Filter(_name, logLevel));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private IDisposable BeginValueTupleScope<T>(ValueTuple<string, T> item)
        {
            return GelfLogScope.Push(new[]
            {
                new KeyValuePair<string, object>(item.Item1, item.Item2)
            });
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            switch (state)
            {
                case ValueTuple<string, string> s:
                    return BeginValueTupleScope(s);
                case ValueTuple<string, sbyte> sb:
                    return BeginValueTupleScope(sb);
                case ValueTuple<string, byte> b:
                    return BeginValueTupleScope(b);
                case ValueTuple<string, short> sh:
                    return BeginValueTupleScope(sh);
                case ValueTuple<string, ushort> us:
                    return BeginValueTupleScope(us);
                case ValueTuple<string, int> i:
                    return BeginValueTupleScope(i);
                case ValueTuple<string, uint> ui:
                    return BeginValueTupleScope(ui);
                case ValueTuple<string, long> l:
                    return BeginValueTupleScope(l);
                case ValueTuple<string, ulong> ul:
                    return BeginValueTupleScope(ul);
                case ValueTuple<string, float> f:
                    return BeginValueTupleScope(f);
                case ValueTuple<string, double> d:
                    return BeginValueTupleScope(d);
                case ValueTuple<string, decimal> dc:
                    return BeginValueTupleScope(dc);
                case IEnumerable<KeyValuePair<string, object>> additionalFields:
                    return GelfLogScope.Push(additionalFields);
                default:
                    return new NoopDisposable();
            }
        }

        private static IEnumerable<KeyValuePair<string, object>> GetStateAdditionalFields<TState>(TState state)
        {
            return state is FormattedLogValues logValues
                ? logValues.Take(logValues.Count - 1)
                : Enumerable.Empty<KeyValuePair<string, object>>();
        }

        private static IEnumerable<KeyValuePair<string, object>> GetScopeAdditionalFields()
        {
            var additionalFields = Enumerable.Empty<KeyValuePair<string, object>>();

            var scope = GelfLogScope.Current;
            while (scope != null)
            {
                additionalFields = additionalFields.Concat(scope.AdditionalFields);
                scope = scope.Parent;
            }

            return additionalFields;
        }

        private static IEnumerable<KeyValuePair<string, object>> ValidateAdditionalFields(
            IEnumerable<KeyValuePair<string, object>> additionalFields)
        {
            foreach (var field in additionalFields)
            {
                if (AdditionalFieldKeyRegex.IsMatch(field.Key) && !ReservedAdditionalFieldKeys.Contains(field.Key))
                {
                    yield return field;
                }
                else
                {
                    Debug.Fail($"GELF message has additional field with invalid key \"{field.Key}\".");
                }
            }
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

        private static double GetTimestamp()
        {
            var totalMiliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var totalSeconds = totalMiliseconds / 1000d;
            return Math.Round(totalSeconds, 2);
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
