using Microsoft.Extensions.Logging;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class GelfLoggerTests
    {
        private readonly GelfLoggerOptions _options;

        public GelfLoggerTests()
        {
            _options = new GelfLoggerOptions
            {
                GelfHost = "localhost",
                GelfPort = 12201,
                AppHost = "Gelf.Extensions.Logging.Tests"
            };
        }

        [Theory]
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
