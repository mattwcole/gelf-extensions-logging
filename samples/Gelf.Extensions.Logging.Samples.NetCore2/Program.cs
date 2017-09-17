using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging.Samples.NetCore2
{
    public class Program
    {
        public static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var serviceCollection = new ServiceCollection()
                .Configure<GelfLoggerOptions>(configuration.GetSection("Logging:GELF"))
                .AddLogging(loggingBuilder => loggingBuilder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddGelf());

            // The LoggerFactory must be disposed before the program exits to ensure all queued messages are sent.
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Information level log from netcoreapp2.0");
                logger.LogDebug("Debug level log from netcoreapp2.0");
                logger.LogTrace("Trace level log from netcoreapp2.0");

                using (logger.BeginScope(("custom_attribute", "12345")))
                {
                    logger.LogInformation("Log with custom attribute from netcoreapp2.0");
                }
            }
        }
    }
}
