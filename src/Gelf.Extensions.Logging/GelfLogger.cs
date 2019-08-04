using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public class GelfLogger : ILogger
    {
        private static readonly Regex AdditionalFieldKeyRegex = new Regex(@"^[\w\.\-]*$", RegexOptions.Compiled);
        private static readonly HashSet<string> ReservedAdditionalFieldKeys = new HashSet<string>
        {
            "id",
            "logger",
            "exception",
            "event_id",
            "event_name",
            "message_template"
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
                .Concat(GetScopeAdditionalFields())
                .Concat(GetStateAdditionalFields(state));

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
                AdditionalFields = ValidateAdditionalFields(additionalFields).ToArray()
            };

            _messageProcessor.SendMessage(message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return logLevel != LogLevel.None && _options.Filter?.Invoke(_name, logLevel) != false;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static IDisposable BeginValueTupleScope<T>(ValueTuple<string, T> item)
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
                case ValueTuple<string, object> o:
                    return BeginValueTupleScope(o);
                case IEnumerable<KeyValuePair<string, object>> additionalFields:
                    return GelfLogScope.Push(additionalFields);
                default:
                    return new NoopDisposable();
            }
        }

        private static IEnumerable<KeyValuePair<string, object>> GetStateAdditionalFields<TState>(TState state)
        {
            return state is IEnumerable<KeyValuePair<string, object>> logValues
                ? logValues
                : Enumerable.Empty<KeyValuePair<string, object>>();
        }

        private IEnumerable<KeyValuePair<string, object>> GetScopeAdditionalFields()
        {
            var additionalFields = Enumerable.Empty<KeyValuePair<string, object>>();

            if (!_options.IncludeScopes)
            {
                return additionalFields;
            }

            var scope = GelfLogScope.Current;
            while (scope != null)
            {
                additionalFields = additionalFields.Concat(scope.AdditionalFields);
                scope = scope.Parent;
            }

            return additionalFields.Reverse();
        }

        private IEnumerable<KeyValuePair<string, object>> ValidateAdditionalFields(
            IEnumerable<KeyValuePair<string, object>> additionalFields)
        {
            foreach (var field in additionalFields)
            {
                if (field.Key != "{OriginalFormat}")
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
                else if (_options.IncludeMessageTemplates)
                {
                    yield return new KeyValuePair<string, object>("message_template", field.Value);
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
            var totalMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var totalSeconds = totalMilliseconds / 1000d;
            return Math.Round(totalSeconds, 3);
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
