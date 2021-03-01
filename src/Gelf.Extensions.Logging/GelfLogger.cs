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

        private static readonly HashSet<string> ReservedAdditionalFieldKeys = new HashSet<string>()
        {
            "id",
            "logger",
            "exception",
            "event_id",
            "event_name",
            "message_template"
        };

        private readonly GelfMessageProcessor _messageProcessor;

        private readonly string _name;
        private readonly GelfLoggerOptions _options;

        public GelfLogger(string name, GelfMessageProcessor messageProcessor, GelfLoggerOptions options)
        {
            _name = name;
            _messageProcessor = messageProcessor;
            _options = options;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = new GelfMessage
            {
                ShortMessage = formatter(state, exception),
                Host = _options.LogSource,
                Level = GetLevel(logLevel),
                Timestamp = GetTimestamp(),
                Logger = _name,
                Exception = exception?.ToString()
            };

            var additionalFields = _options.AdditionalFields
                .Concat(GetFactoryAdditionalFields(logLevel, eventId, exception))
                .Concat(GetScopeAdditionalFields())
                .Concat(GetStateAdditionalFields(state));

            message.AdditionalFields = ValidateAdditionalFields(additionalFields).ToArray();

            if (eventId != default)
            {
                message.EventId = eventId.Id;
                message.EventName = eventId.Name;
            }

            _messageProcessor.SendMessage(message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return state switch
            {
                IEnumerable<KeyValuePair<string, object>> fields => GelfLogScope.Push(fields),
                ValueTuple<string, string> field => BeginValueTupleScope(field),
                ValueTuple<string, sbyte> field => BeginValueTupleScope(field),
                ValueTuple<string, byte> field => BeginValueTupleScope(field),
                ValueTuple<string, short> field => BeginValueTupleScope(field),
                ValueTuple<string, ushort> field => BeginValueTupleScope(field),
                ValueTuple<string, int> field => BeginValueTupleScope(field),
                ValueTuple<string, uint> field => BeginValueTupleScope(field),
                ValueTuple<string, long> field => BeginValueTupleScope(field),
                ValueTuple<string, ulong> field => BeginValueTupleScope(field),
                ValueTuple<string, float> field => BeginValueTupleScope(field),
                ValueTuple<string, double> field => BeginValueTupleScope(field),
                ValueTuple<string, decimal> field => BeginValueTupleScope(field),
                ValueTuple<string, object> field => BeginValueTupleScope(field),
                _ => new NoopDisposable()
            };

            static IDisposable BeginValueTupleScope((string, object) field)
            {
                return GelfLogScope.Push(new[]
                {
                    new KeyValuePair<string, object>(field.Item1, field.Item2)
                });
            }
        }

        private IEnumerable<KeyValuePair<string, object>> GetFactoryAdditionalFields(
            LogLevel logLevel, EventId eventId, Exception? exception)
        {
            return _options.AdditionalFieldsFactory?.Invoke(logLevel, eventId, exception) ??
                   Enumerable.Empty<KeyValuePair<string, object>>();
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

        private static IEnumerable<KeyValuePair<string, object>> GetStateAdditionalFields<TState>(TState state)
        {
            return state is IEnumerable<KeyValuePair<string, object>> logValues
                ? logValues
                : Enumerable.Empty<KeyValuePair<string, object>>();
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
            return logLevel switch
            {
                LogLevel.Trace => SyslogSeverity.Debug,
                LogLevel.Debug => SyslogSeverity.Debug,
                LogLevel.Information => SyslogSeverity.Informational,
                LogLevel.Warning => SyslogSeverity.Warning,
                LogLevel.Error => SyslogSeverity.Error,
                LogLevel.Critical => SyslogSeverity.Critical,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Log level not supported.")
            };
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
