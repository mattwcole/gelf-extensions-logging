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
        private static readonly Regex AdditionalFieldKeyRegex = new(@"^[\w\.\-]*$", RegexOptions.Compiled);

        private readonly string _name;
        private readonly GelfMessageProcessor _messageProcessor;

        public GelfLogger(string name, GelfMessageProcessor messageProcessor, GelfLoggerOptions options)
        {
            _name = name;
            _messageProcessor = messageProcessor;
            Options = options;
        }

        internal IExternalScopeProvider? ScopeProvider { get; set; }
        internal GelfLoggerOptions Options { get; set; }

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
                Host = Options.LogSource,
                Logger = Options.IncludeDefaultFields ? _name : null,
                Exception = Options.IncludeDefaultFields ? exception?.ToString() : null,
                Level = GetLevel(logLevel),
                Timestamp = GetTimestamp(),
                AdditionalFields = GetAdditionalFields(logLevel, eventId, state, exception).ToArray()
            };

            if (eventId != default && Options.IncludeDefaultFields)
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

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

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

        private IEnumerable<KeyValuePair<string, object>> GetAdditionalFields<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception)
        {
            var additionalFields = Options.AdditionalFields
                .Concat(GetFactoryAdditionalFields(_name, logLevel, eventId, exception))
                .Concat(GetScopeAdditionalFields())
                .Concat(GetStateAdditionalFields(state));

            foreach (var field in additionalFields)
            {
                if (field.Key != "{OriginalFormat}")
                {
                    if (AdditionalFieldKeyRegex.IsMatch(field.Key) && !IsReservedAdditionalFieldKey(field.Key))
                    {
                        yield return field;
                    }
                    else
                    {
                        Debug.Fail($"GELF message has additional field with invalid key \"{field.Key}\".");
                    }
                }
                else if (Options.IncludeMessageTemplates)
                {
                    yield return new KeyValuePair<string, object>("message_template", field.Value);
                }
            }
        }

        private bool IsReservedAdditionalFieldKey(string key)
        {
            if (key == "id")
            {
                return true;
            }

            if (Options.IncludeDefaultFields &&
                (key == "logger" || key == "exception" || key == "event_id" || key == "event_name"))
            {
                return true;
            }

            if (Options.IncludeMessageTemplates && key == "message_template")
            {
                return true;
            }

            return false;
        }

        private IEnumerable<KeyValuePair<string, object>> GetFactoryAdditionalFields(
            string loggerName, LogLevel logLevel, EventId eventId, Exception? exception)
        {
            return Options.AdditionalFieldsFactory?.Invoke(new GelfLoggerContext(loggerName, logLevel, eventId, exception)) ??
                   Enumerable.Empty<KeyValuePair<string, object>>();
        }

        private IEnumerable<KeyValuePair<string, object>> GetScopeAdditionalFields()
        {
            if (!Options.IncludeScopes)
            {
                return Enumerable.Empty<KeyValuePair<string, object>>();
            }

            var additionalFields = new List<KeyValuePair<string, object>>();

            ScopeProvider?.ForEachScope((scope, state) =>
            {
                switch (scope)
                {
                    case IEnumerable<KeyValuePair<string, object>> fields:
                        state.AddRange(fields);
                        break;
                    case ValueTuple<string, string>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, int>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, long>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, short>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, decimal>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, double>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, float>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, uint>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, ulong>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, ushort>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, byte>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, sbyte>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                    case ValueTuple<string, object>(var key, var value):
                        state.Add(new KeyValuePair<string, object>(key, value));
                        break;
                }
            }, additionalFields);

            return additionalFields;
        }

        private static IEnumerable<KeyValuePair<string, object>> GetStateAdditionalFields<TState>(TState state)
        {
            return state is IEnumerable<KeyValuePair<string, object>> additionalFields
                ? additionalFields
                : Enumerable.Empty<KeyValuePair<string, object>>();
        }
    }
}
