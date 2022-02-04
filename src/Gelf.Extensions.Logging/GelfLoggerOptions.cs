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
        ///     Set maximum duration to wait for TCP requests to wait before cancelling (Default 1000).
        /// </summary>
        public int TcpTimeoutMs { get; set; } = 1000;

        /// <summary>
        ///     If set to true, TCP errors will be thrown to the caller. (Default false).
        /// </summary>
        public bool ThrowTcpExceptions { get; set; } = true;

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
    }
}
