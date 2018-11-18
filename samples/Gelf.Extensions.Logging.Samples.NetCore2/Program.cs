using System;
using System.Collections.Generic;
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
                .AddJsonFile("appsettings.json")
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddGelf(options => options.AdditionalFields["machine_name"] = Environment.MachineName));

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

            using (logger.BeginScope(("scope_field_1", "foo")))
            {
                logger.LogDebug("Debug log from {framework}", framework);

                using (logger.BeginScope(new Dictionary<string, object>
                {
                    ["scope_field_2"] = "bar",
                    ["scope_field_3"] = "baz"
                }))
                {
                    logger.LogTrace("Trace log from {framework}", framework);
                }

                logger.LogError(new EventId(), new Exception("Example exception!"),
                    "Error log from {framework}", framework);
            }
        }
    }
}
