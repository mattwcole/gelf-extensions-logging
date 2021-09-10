using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        /// <returns></returns>
        public static ILoggerFactory AddGelf(
            this ILoggerFactory loggerFactory, IOptionsMonitor<GelfLoggerOptions> options)
        {
            loggerFactory.AddProvider(new GelfLoggerProvider(options));
            return loggerFactory;
        }

        /// <summary>
        ///     Adds a <see cref="GelfLoggerProvider" /> to the logger factory with the supplied
        ///     <see cref="GelfLoggerOptions" />.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ILoggerFactory AddGelf(this ILoggerFactory loggerFactory, GelfLoggerOptions options)
        {
            return loggerFactory.AddGelf(new OptionsMonitorStub<GelfLoggerOptions>(options));
        }

        private class OptionsMonitorStub<T> : IOptionsMonitor<T>
        {
            public OptionsMonitorStub(T options)
            {
                CurrentValue = options;
            }

            public T CurrentValue { get; }

            public T Get(string name) => CurrentValue;

            public IDisposable OnChange(Action<T, string> listener) => new NullDisposable();

            private class NullDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
