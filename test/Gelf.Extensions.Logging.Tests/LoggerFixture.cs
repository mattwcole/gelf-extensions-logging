using System;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging.Tests
{
    public class LoggerFixture : IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;

        public LoggerFixture()
        {
            TestContext.TestId = Guid.NewGuid().ToString();
            
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddGelf(LoggerOptions);
        }

        public GelfLoggerOptions LoggerOptions => new GelfLoggerOptions
        {
            Host = GraylogFixture.GraylogHost,
            Port = GraylogFixture.GraylogLogPort,
            LogSource = "Gelf.Extensions.Logging.Tests"
        };

        public ILoggerFactory CreateLoggerFactory(GelfLoggerOptions options)
        {
            var loggerFactory = new LoggerFactory();
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
}
