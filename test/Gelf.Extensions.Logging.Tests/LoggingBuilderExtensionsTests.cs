using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gelf.Extensions.Logging.Tests;

public class LoggingBuilderExtensionsTests
{
    [Fact]
    public void Reads_GELF_logger_options_from_logging_configuration_section()
    {
        var configuration = new ConfigurationBuilder().Add(new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string>
            {
                ["Logging:GELF:IncludeScopes"] = "false",
                ["Logging:GELF:Protocol"] = "HTTP",
                ["Logging:GELF:Host"] = "graylog-host"
            }
        }).Build();

        var serviceCollection = new ServiceCollection()
            .AddLogging(loggingBuilder => loggingBuilder
                .AddConfiguration(configuration.GetSection("Logging"))
                .AddGelf(o => o.LogSource = "post-configured-log-source"));

        using var provider = serviceCollection.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<GelfLoggerOptions>>();

        Assert.False(options.Value.IncludeScopes);
        Assert.Equal(GelfProtocol.Http, options.Value.Protocol);
        Assert.Equal("graylog-host", options.Value.Host);
        Assert.Equal("post-configured-log-source", options.Value.LogSource);
    }

    [Fact]
    public void Reads_GELF_logger_options_default_values()
    {
        var configuration = new ConfigurationBuilder().Build();

        var serviceCollection = new ServiceCollection()
            .AddLogging(loggingBuilder => loggingBuilder
                .AddConfiguration(configuration.GetSection("Logging"))
                .AddGelf());

        using var provider = serviceCollection.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<GelfLoggerOptions>>();

        Assert.Equal(GelfProtocol.Udp, options.Value.Protocol);
        Assert.Equal(12201, options.Value.Port);
        Assert.Equal(512, options.Value.UdpCompressionThreshold);
        Assert.Equal(8192, options.Value.UdpMaxChunkSize);
        Assert.Equal(TimeSpan.FromSeconds(30), options.Value.HttpTimeout);
        Assert.True(options.Value.IncludeScopes);
        Assert.True(options.Value.CompressUdp);
        Assert.False(options.Value.IncludeMessageTemplates);
    }
}