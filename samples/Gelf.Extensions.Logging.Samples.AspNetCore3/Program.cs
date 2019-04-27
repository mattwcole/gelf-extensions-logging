using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Gelf.Extensions.Logging.Samples.AspNetCore3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
.ConfigureLogging((context, builder) =>
{
    builder.AddConfiguration(context.Configuration.GetSection("Logging"))
    .AddConsole()
    .AddDebug()
    .AddGelf(options =>
    {
    // Optional config combined with Logging:GELF configuration section.
    options.LogSource = context.HostingEnvironment.ApplicationName;
        options.AdditionalFields["machine_name"] = Environment.MachineName;
        options.AdditionalFields["app_version"] = Assembly.GetEntryAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    });
})
.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.UseStartup<Startup>();
});
        }
    }
}
