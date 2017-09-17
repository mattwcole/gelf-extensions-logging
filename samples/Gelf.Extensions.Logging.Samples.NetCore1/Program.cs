using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging.Samples.NetCore1
{
    public class Program
    {
        public static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            try
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                loggerFactory
                    .AddConsole(configuration.GetSection("Logging:Console"))
                    .AddGelf(configuration.GetSection("Logging:GELF").Get<GelfLoggerOptions>());

                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Information level log from netcoreapp1.1");
                logger.LogDebug("Debug level log from netcoreapp1.1");
                logger.LogTrace("Trace level log from netcoreapp1.1");

                using (logger.BeginScope(("custom_attribute", "12345")))
                {
                    logger.LogInformation("Log with custom attribute from netcoreapp1.1");
                }
            }
            finally
            {
                // The LoggerFactory must be disposed before the program exits to ensure all queued messages are sent.
                (serviceProvider as IDisposable)?.Dispose();
            }
        }
    }
}
