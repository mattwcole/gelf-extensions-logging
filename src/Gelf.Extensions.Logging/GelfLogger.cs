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
        private static readonly HashSet<string> ReservedAdditionalFieldKeys = new() { "id" };

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
                Level = GetLevel(logLevel),
                Timestamp = GetTimestamp(),
                AdditionalFields = GetAdditionalFields(logLevel, eventId, state, exception).ToArray()
            };

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

        private IEnumerable<KeyValuePair<string, object?>> GetAdditionalFields<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception)
        {
            var logContext = new GelfLogContext(_name, logLevel, eventId, exception);

            var additionalFields = Options.AdditionalFields
                .Concat(GetFactoryAdditionalFields(logContext))
                .Concat(GetScopeAdditionalFields())
                .Concat(GetStateAdditionalFields(state))
                .Concat(GetDefaultAdditionalFields(logContext));

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

        private IEnumerable<KeyValuePair<string, object?>> GetFactoryAdditionalFields(GelfLogContext logContext)
        {
            return Options.AdditionalFieldsFactory?.Invoke(logContext) ??
                   Enumerable.Empty<KeyValuePair<string, object?>>();
        }

        private IEnumerable<KeyValuePair<string, object?>> GetScopeAdditionalFields()
        {
            if (!Options.IncludeScopes)
            {
                return Enumerable.Empty<KeyValuePair<string, object?>>();
            }

            var additionalFields = new List<KeyValuePair<string, object?>>();

            ScopeProvider?.ForEachScope((scope, state) =>
            {
                switch (scope)
                {
                    case IEnumerable<KeyValuePair<string, object?>> fields:
                        state.AddRange(fields);
                        break;
                    case ValueTuple<string, string>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, int>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, long>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, short>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, decimal>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, double>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, float>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, uint>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, ulong>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, ushort>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, byte>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, sbyte>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                    case ValueTuple<string, object?>(var key, var value):
                        state.Add(new KeyValuePair<string, object?>(key, value));
                        break;
                }
            }, additionalFields);

            return additionalFields;
        }

        private IEnumerable<KeyValuePair<string, object?>> GetStateAdditionalFields<TState>(TState state)
        {
            var additionalFields = state as IEnumerable<KeyValuePair<string, object?>>
                                   ?? Enumerable.Empty<KeyValuePair<string, object?>>();

            foreach (var field in additionalFields)
            {
                if (field.Key != "{OriginalFormat}")
                {
                    yield return field;
                }
                else if (Options.IncludeMessageTemplates)
                {
                    yield return new KeyValuePair<string, object?>("message_template", field.Value);
                }
            }
        }

        private IEnumerable<KeyValuePair<string, object?>> GetDefaultAdditionalFields(GelfLogContext logContext)
        {
            if (!Options.IncludeDefaultFields)
            {
                yield break;
            }

            yield return new KeyValuePair<string, object?>("logger", logContext.LoggerName);

            if (logContext.Exception != null)
            {
                yield return new KeyValuePair<string, object?>("exception", logContext.Exception);
            }

            if (logContext.EventId != default)
            {
                yield return new KeyValuePair<string, object?>("event_id", logContext.EventId.Id);
                yield return new KeyValuePair<string, object?>("event_name", logContext.EventId.Name);
            }
        }
    }
}
