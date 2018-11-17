using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public class GelfLoggerOptions
    {
        public GelfLoggerOptions()
        {
#if !NETSTANDARD2_0
            Filter = (name, level) => level >= LogLevel;
#endif
        }

        /// <summary>
        /// Enable/disable additional fields added via log scopes.
        /// </summary>
        public bool IncludeScopes { get; set; } = true;

        /// <summary>
        /// Protocol used to send logs.
        /// </summary>
        public GelfProtocol Protocol { get; set; } = GelfProtocol.Udp;

        /// <summary>
        /// GELF server host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// GELF server port.
        /// </summary>
        public int Port { get; set; } = 12201;

        /// <summary>
        /// Log source name mapped to the GELF host field (required).
        /// </summary>
        public string LogSource { get; set; }

        /// <summary>
        /// Enable GZip message compression for UDP logging.
        /// </summary>
        public bool CompressUdp { get; set; } = true;

        /// <summary>
        /// The UDP message size in bytes under which messages will not be compressed.
        /// </summary>
        public int UdpCompressionThreshold { get; set; } = 512;

        /// <summary>
        /// Function used to filter log events based on logger name and level. Uses <see cref="LogLevel"/> by default.
        /// </summary>
#if NETSTANDARD2_0
        [Obsolete("Filter logs with app config or Microsoft.Extensions.Logging.FilterLoggingBuilderExtensions.")]
#endif
        public Func<string, LogLevel, bool> Filter { get; set; }

        /// <summary>
        /// The log level used by the default filter. This is ignored if <see cref="Filter"/> is overridden.
        /// </summary>
#if NETSTANDARD2_0
        [Obsolete("Filter logs with app config or Microsoft.Extensions.Logging.FilterLoggingBuilderExtensions.")]
#endif
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Additional fields that will be attached to all log messages.
        /// </summary>
        public Dictionary<string, object> AdditionalFields { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Headers used when sending logs via HTTP(S).
        /// </summary>
        public Dictionary<string, string> HttpHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Timeout used when sending logs via HTTP(S).
        /// </summary>
        public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
