using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging.Samples.AspNetCore2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((context, builder) =>
                {
                    // Read GelfLoggerOptions from appsettings.json.
                    builder.Services.Configure<GelfLoggerOptions>(context.Configuration.GetSection("Graylog"));

                    // Optionally configure GelfLoggerOptions further.
                    builder.Services.PostConfigure<GelfLoggerOptions>(options =>
                        options.AdditionalFields["machine_name"] = Environment.MachineName);

                    // Read Logging settings from appsettings.json and add providers.
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"))
                        .AddConsole()
                        .AddDebug()
                        .AddGelf();
                })
                .Build();
        }
    }
}
