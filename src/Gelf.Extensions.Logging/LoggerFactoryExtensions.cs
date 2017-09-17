using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public static class LoggerFactoryExtensions
    {
        /// <summary>
        /// Adds a <see cref="GelfLoggerProvider"/> to the logger factory with the supplied
        /// <see cref="GelfLoggerOptions"/>.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ILoggerFactory AddGelf(this ILoggerFactory loggerFactory, GelfLoggerOptions options)
        {
            loggerFactory.AddProvider(new GelfLoggerProvider(options));
            return loggerFactory;
        }
    }
}
