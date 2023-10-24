using System.Threading.Tasks;
using Gelf.Extensions.Logging.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Gelf.Extensions.Logging.Tests
{
    public class UdpGelfLoggerTests : GelfLoggerTests, IClassFixture<UdpGraylogFixture>
    {
        public UdpGelfLoggerTests(UdpGraylogFixture graylogFixture) : base(graylogFixture,
            new LoggerFixture(new GelfLoggerOptions
            {
                Host = GraylogFixture.Host,
                Port = graylogFixture.InputPort,
                Protocol = GelfProtocol.Udp,
                LogSource = nameof(UdpGelfLoggerTests)
            }))
        {
        }

        [Theory]
        [InlineData(50, 100)]
        [InlineData(200, 100)]
        [InlineData(300, 300)]
        [InlineData(23000, 25000)]
        [InlineData(12000, 10000)]
        public async Task Sends_message_with_and_without_compression(int compressionThreshold, int messageSize)
        {
            var options = LoggerFixture.LoggerOptions;
            options.UdpCompressionThreshold = compressionThreshold;
            var messageText = new string('*', messageSize);

            using var loggerFactory = LoggerFixture.CreateLoggerFactory(options);
            var sut = loggerFactory.CreateLogger(nameof(GelfLoggerTests));
            sut.LogInformation(messageText);

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.NotEmpty(message._id);
            Assert.Equal(options.LogSource, message.source);
            Assert.Equal(messageText, message.message);
            Assert.Equal(6, message.level);
        }

        [Fact]
        public async Task Sends_message_chunks_larger_than_MTU()
        {
            var options = LoggerFixture.LoggerOptions;
            options.CompressUdp = false;
            options.UdpMaxChunkSize = int.MaxValue;
            var messageText = new string('*', 1600);    // Assumes MTU under 1600 when running in Docker

            using var loggerFactory = LoggerFixture.CreateLoggerFactory(options);
            var sut = loggerFactory.CreateLogger(nameof(GelfLoggerTests));
            sut.LogInformation(messageText);

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.NotEmpty(message._id);
            Assert.Equal(messageText, message.message);
        }
    }
}
