using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class Sandbox
    {
        private readonly GelfLoggerOptions _options;

        public Sandbox()
        {
            _options = new GelfLoggerOptions
            {
                Host = "localhost",
                Port = 12201,
                LogSource = "Gelf.Extensions.Logging.Tests",
                LogLevel = LogLevel.Trace
            };
        }

        [Theory(Skip = "Requires Graylog server with UDP input.")]
        [InlineData(50, 100)]
        [InlineData(200, 100)]
        [InlineData(30000, 40000)]
        [InlineData(12000, 10000)]
        public void Sends_messages_with_and_without_compression(int compressionThreshold, int messageSize)
        {
            _options.CompressionThreshold = compressionThreshold;

            using (var loggerProvider = new GelfLoggerProvider(_options))
            {
                var logger = loggerProvider.CreateLogger("Tests");
                logger.LogInformation(new string('@', messageSize));
            }
        }

        [Fact(Skip = "Requires Graylog server with UDP input.")]
        public void Sends_message_with_additional_fields_from_scope()
        {
            using (var loggerProvider = new GelfLoggerProvider(_options))
            {
                var logger = loggerProvider.CreateLogger("Tests.Scope");
                using (logger.BeginScope(("foo", "123456")))
                {
                    logger.LogInformation("Message with foo field");

                    using (logger.BeginScope(new Dictionary<string, string>
                    {
                        ["bar1"] = "abcdef",
                        ["bar2"] = "qwerty"
                    }))
                    {
                        logger.LogWarning("Message with foo and multiple bar fields");
                    }
                }
            }
        }
    }
}
