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
        /// GELF server host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// GELF server port.
        /// </summary>
        public int Port { get; set; } = 12201;

        /// <summary>
        /// Log source name mapped to the GELF host field.
        /// </summary>
        public string LogSource { get; set; }

        /// <summary>
        /// Enable GZip message compression.
        /// </summary>
        public bool Compress { get; set; } = true;

        /// <summary>
        /// The message size in bytes under which messages will not be compressed.
        /// </summary>
        public int CompressionThreshold { get; set; } = 512;

        /// <summary>
        /// Function used to filter log events based on logger name and level. Uses <see cref="LogLevel"/> by default.
        /// </summary>
#if NETSTANDARD2_0
        [Obsolete("Logs should be filtered using LoggerFactory.")]
#endif
        public Func<string, LogLevel, bool> Filter { get; set; }

        /// <summary>
        /// The log level used by the default filter. This is ignored if <see cref="Filter"/> is customised.
        /// </summary>
#if NETSTANDARD2_0
        [Obsolete("Logs should be filtered using LoggerFactory.")]
#endif
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Additional fields that will be attached to all log messages.
        /// </summary>
        public Dictionary<string, string> AdditionalFields { get; } = new Dictionary<string, string>();
    }
}
