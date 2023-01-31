using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public class GelfLoggerOptions
    {
        /// <summary>
        ///     Enable/disable additional fields added via log scopes.
        /// </summary>
        public bool IncludeScopes { get; set; } = true;

        /// <summary>
        ///     Protocol used to send logs.
        /// </summary>
        public GelfProtocol Protocol { get; set; } = GelfProtocol.Udp;

        /// <summary>
        ///     GELF server host.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        ///     GELF server port.
        /// </summary>
        public int Port { get; set; } = 12201;

        /// <summary>
        ///     Log source name mapped to the GELF host field (required).
        /// </summary>
        public string? LogSource { get; set; }

        /// <summary>
        ///     Enable GZip message compression for UDP logging.
        /// </summary>
        public bool CompressUdp { get; set; } = true;

        /// <summary>
        ///     The UDP message size in bytes under which messages will not be compressed.
        /// </summary>
        public int UdpCompressionThreshold { get; set; } = 512;

        /// <summary>
        ///     The UDP message max size in bytes to be sent in one datagram.
        /// </summary>
        public int UdpMaxChunkSize { get; set; } = 8192;

        /// <summary>
        ///     Additional fields that will be attached to all log messages.
        /// </summary>
        public Dictionary<string, object> AdditionalFields { get; set; } = new();

        /// <summary>
        ///     Additional fields computed based on raw log data.
        /// </summary>
        public Func<LogLevel, EventId, Exception?, Dictionary<string, object>>? AdditionalFieldsFactory { get; set; }

        /// <summary>
        ///     Headers used when sending logs via HTTP(S).
        /// </summary>
        public Dictionary<string, string> HttpHeaders { get; set; } = new();

        /// <summary>
        ///     Timeout used when sending logs via HTTP(S).
        /// </summary>
        public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     Include a field with the original message template before structured log parameters are replaced.
        /// </summary>
        public bool IncludeMessageTemplates { get; set; }

        /// <summary>
        ///     The field key to use for the logger name, or null to omit.
        /// </summary>
        public string? LoggerFieldKey
        {
            get => _loggerField.Key;
            set => _loggerField = FieldFromValue(value);
        }

        /// <summary>
        ///     The field key to use for the exception details, or null to omit.
        /// </summary>
        public string? ExceptionFieldKey
        {
            get => _exceptionField.Key;
            set => _exceptionField = FieldFromValue(value);
        }

        /// <summary>
        ///     The field key to use for the event ID, or null to omit.
        /// </summary>
        public string? EventIdFieldKey
        {
            get => _eventIdField.Key;
            set => _eventIdField = FieldFromValue(value);
        }

        /// <summary>
        ///     The field key to use for the event name, or null to omit.
        /// </summary>
        public string? EventNameFieldKey
        {
            get => _eventNameField.Key;
            set => _eventNameField = FieldFromValue(value);
        }

        internal string? LoggerPropertyName => _loggerField.Name;
        internal string? ExceptionPropertyName => _exceptionField.Name;
        internal string? EventIdPropertyName => _eventIdField.Name;
        internal string? EventNamePropertyName => _eventNameField.Name;

        private static (string? Key, string? Name) FieldFromValue(string? value) => value == null ? default : (value, $"_{value}");

        private (string? Key, string? Name) _loggerField = FieldFromValue("logger");
        private (string? Key, string? Name) _exceptionField = FieldFromValue("exception");
        private (string? Key, string? Name) _eventIdField = FieldFromValue("event_id");
        private (string? Key, string? Name) _eventNameField = FieldFromValue("event_name");
    }
}
