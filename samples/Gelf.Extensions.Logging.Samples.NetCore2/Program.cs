using System;
using System.Collections.Generic;
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
                .Configure<GelfLoggerOptions>(configuration.GetSection("Graylog"))
                .AddLogging(loggingBuilder => loggingBuilder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddGelf());

            // The LoggerFactory must be disposed before the program exits to ensure all queued messages are sent.
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                UseLogger(serviceProvider.GetRequiredService<ILogger<Program>>());
            }
        }

        private static void UseLogger(ILogger<Program> logger)
        {
            const string framework = "netcoreapp2.0";

            logger.LogInformation("Information log from {framework}", framework);

            using (logger.BeginScope(("scope_field1", "foo")))
            {
                logger.LogDebug("Debug log from {framework}", framework);

                using (logger.BeginScope(new Dictionary<string, object>
                {
                    ["scope_field2"] = "bar",
                    ["scope_field3"] = "baz"
                }))
                {
                    logger.LogTrace("Debug log from {framework}", framework);
                }

                logger.LogError(new EventId(), new Exception("Example exception!"),
                    "Error log from {framework}", framework);
            }
        }
    }
}
