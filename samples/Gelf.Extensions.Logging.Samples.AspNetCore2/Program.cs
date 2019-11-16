using System;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Gelf.Extensions.Logging.Samples.AspNetCore2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging((context, builder) => builder.AddGelf(options =>
            {
                // Optional config combined with Logging:GELF configuration section.
                options.LogSource = context.HostingEnvironment.ApplicationName;
                options.AdditionalFields["machine_name"] = Environment.MachineName;
                options.AdditionalFields["app_version"] = Assembly.GetEntryAssembly()
                    ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            }));
    }
}
