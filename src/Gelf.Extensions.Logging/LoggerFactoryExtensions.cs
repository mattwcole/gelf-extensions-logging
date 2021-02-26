using System;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public static class LoggerFactoryExtensions
    {
        /// <summary>
        ///     Adds a <see cref="GelfLoggerProvider" /> to the logger factory with the supplied
        ///     <see cref="GelfLoggerOptions" />.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="options"></param>
        /// <param name="exceptionHandler"></param>
        /// <returns></returns>
        public static ILoggerFactory AddGelf(
            this ILoggerFactory loggerFactory,
            GelfLoggerOptions options,
            Action<Exception> exceptionHandler = null)
        {
            loggerFactory.AddProvider(new GelfLoggerProvider(options, exceptionHandler));
            return loggerFactory;
        }
    }
}
