using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddGelf(this ILoggerFactory loggerFactory, GelfLoggerOptions options)
        {
            loggerFactory.AddProvider(new GelfLoggerProvider(options));
            return loggerFactory;
        }
    }
}
