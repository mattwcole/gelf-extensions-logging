using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class UdpGelfLoggerTests : GelfLoggerTests, IClassFixture<UdpGraylogFixture>
    {
        public UdpGelfLoggerTests(UdpGraylogFixture graylogFixture) : base(graylogFixture,
            new LoggerFixture(new GelfLoggerOptions
            {
                Host = GraylogFixture.GraylogHost,
                Port = graylogFixture.InputPort,
                Protocol = GelfProtocol.Udp,
                LogSource = typeof(UdpGelfLoggerTests).Name
            }))
        {
        }
    }
}
