using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class LoggingBuilderExtensionsTests
    {
        [Fact]
        public void Reads_GELF_logger_options_from_logging_configuration_section_by_default()
        {
            var configuration = new ConfigurationBuilder().Add(new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string>
                {
                    ["Logging:GELF:IncludeScopes"] = "false",
                    ["Logging:GELF:Protocol"] = "HTTP",
                    ["Logging:GELF:Host"] = "graylog-host-1"
                }
            }).Build();

            var serviceCollection = new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddGelf(o => o.LogSource = "post-configured-log-source"));

            using (var provider = serviceCollection.BuildServiceProvider())
            {
                var options = provider.GetRequiredService<IOptions<GelfLoggerOptions>>();

                Assert.False(options.Value.IncludeScopes);
                Assert.Equal(GelfProtocol.Http, options.Value.Protocol);
                Assert.Equal("graylog-host-1", options.Value.Host);
                Assert.Equal("post-configured-log-source", options.Value.LogSource);
            }
        }

        [Fact]
        public void Reads_GELF_logger_options_from_custom_configuration_section()
        {
            var configuration = new ConfigurationBuilder().Add(new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string>
                {
                    ["Logging:GELF:IncludeScopes"] = "true",
                    ["Logging:GELF:Protocol"] = "HTTP",
                    ["Logging:GELF:Host"] = "graylog-host-1",
                    ["Graylog:IncludeScopes"] = "false",
                    ["Graylog:Protocol"] = "HTTPS",
                    ["Graylog:Host"] = "graylog-host-2"
                }
            }).Build();

            var serviceCollection = new ServiceCollection()
                .Configure<GelfLoggerOptions>(configuration.GetSection("Graylog"))
                .PostConfigure<GelfLoggerOptions>(o => o.LogSource = "post-configured-log-source")
                .AddLogging(loggingBuilder => loggingBuilder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddGelf(o => o.LogSource = "post-configured-log-source"));

            using (var provider = serviceCollection.BuildServiceProvider())
            {
                var options = provider.GetRequiredService<IOptions<GelfLoggerOptions>>();

                Assert.False(options.Value.IncludeScopes);
                Assert.Equal(GelfProtocol.Https, options.Value.Protocol);
                Assert.Equal("graylog-host-2", options.Value.Host);
                Assert.Equal("post-configured-log-source", options.Value.LogSource);
            }
        }
    }
}
