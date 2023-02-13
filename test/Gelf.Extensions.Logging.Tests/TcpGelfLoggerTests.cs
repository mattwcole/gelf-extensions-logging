using Gelf.Extensions.Logging.Tests.Fixtures;
using Xunit;

namespace Gelf.Extensions.Logging.Tests;

public class TcpGelfLoggerTests : GelfLoggerTests, IClassFixture<TcpGraylogFixture>
{
    public TcpGelfLoggerTests(TcpGraylogFixture graylogFixture) : base(graylogFixture,
        new LoggerFixture(new GelfLoggerOptions
        {
            Host = GraylogFixture.Host,
            Port = graylogFixture.InputPort,
            Protocol = GelfProtocol.Tcp,
            LogSource = nameof(TcpGelfLoggerTests)
        }))
    {
    }
}