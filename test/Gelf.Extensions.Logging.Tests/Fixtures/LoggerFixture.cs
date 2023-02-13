using System;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging.Tests.Fixtures;

public class LoggerFixture : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;

    public LoggerFixture(GelfLoggerOptions options)
    {
        TestContext.TestId = Guid.NewGuid().ToString();

        LoggerOptions = options;
        _loggerFactory = new LoggerFactory();
        _loggerFactory.AddGelf(LoggerOptions);
    }

    public GelfLoggerOptions LoggerOptions { get; }

    public ILoggerFactory CreateLoggerFactory(GelfLoggerOptions options,
        ActivityTrackingOptions trackingOptions = ActivityTrackingOptions.None)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder
            .Configure(loggerOptions => loggerOptions
                .ActivityTrackingOptions = trackingOptions));

        loggerFactory.AddGelf(options);

        var logger = loggerFactory.CreateLogger<LoggerFixture>();
        logger.BeginScope(("test_id", TestContext.TestId));

        return loggerFactory;
    }

    public ILogger<T> CreateLogger<T>()
    {
        var logger = _loggerFactory.CreateLogger<T>();
        logger.BeginScope(("test_id", TestContext.TestId));
        return logger;
    }

    public void Dispose()
    {
        _loggerFactory.Dispose();
    }
}