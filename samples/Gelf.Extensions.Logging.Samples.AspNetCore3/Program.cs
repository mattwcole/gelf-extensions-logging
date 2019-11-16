using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Gelf.Extensions.Logging.Samples.AspNetCore3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .ConfigureLogging((context, builder) => builder.AddGelf(options =>
                        {
                            // Optional config combined with Logging:GELF configuration section.
                            options.LogSource = context.HostingEnvironment.ApplicationName;
                            options.AdditionalFields["machine_name"] = Environment.MachineName;
                            options.AdditionalFields["app_version"] = Assembly.GetEntryAssembly()
                                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                        }));
                });
    }
}
