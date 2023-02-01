using System;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public class GelfLoggerContext
    {
        /// <summary>
        ///     The logger name.
        /// </summary>
        public string LoggerName { get; }

        /// <summary>
        ///     The logging level.
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        ///     The event ID.
        /// </summary>
        public EventId EventId { get; }

        /// <summary>
        ///     The exception, if any.
        /// </summary>
        public Exception? Exception { get; }

        internal GelfLoggerContext(string loggerName, LogLevel logLevel, EventId eventId, Exception? exception)
        {
            LoggerName = loggerName;
            LogLevel = logLevel;
            EventId = eventId;
            Exception = exception;
        }
    }
}
