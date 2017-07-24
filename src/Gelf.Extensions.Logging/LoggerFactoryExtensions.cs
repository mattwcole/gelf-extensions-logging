using System;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddGelf(this ILoggerFactory loggerFactory, GelfLoggerOptions options)
        {
            if (string.IsNullOrEmpty(options.Host))
            {
                throw new ArgumentException("GELF host is required.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.LogSource))
            {
                throw new ArgumentException("Application host/source is required.", nameof(options));
            }

            loggerFactory.AddProvider(new GelfLoggerProvider(options));
            return loggerFactory;
        }
    }
}
