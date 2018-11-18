#if NETSTANDARD2_0

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Gelf.Extensions.Logging
{
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Registers a <see cref="GelfLoggerProvider"/> with the service collection.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddGelf(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.AddSingleton<ILoggerProvider, GelfLoggerProvider>();
            builder.Services.TryAddSingleton<IConfigureOptions<GelfLoggerOptions>, GelfLoggerOptionsSetup>();

            return builder;
        }

        /// <summary>
        /// Registers a <see cref="GelfLoggerProvider"/> with the service collection allowing logger options
        /// to be customised.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddGelf(this ILoggingBuilder builder, Action<GelfLoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddGelf();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}

#endif
