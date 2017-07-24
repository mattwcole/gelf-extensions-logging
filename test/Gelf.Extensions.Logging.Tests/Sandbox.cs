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
                LogSource = "Gelf.Extensions.Logging.Tests"
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
    }
}
