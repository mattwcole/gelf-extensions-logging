using Xunit.Abstractions;

namespace Gelf.Extensions.Logging.Tests.Fixtures
{
    public class UdpGraylogFixture : GraylogFixture
    {
        public UdpGraylogFixture(IMessageSink messageSink) : base(messageSink)
        {
        }

        public override int InputPort => 12201;

        protected override string InputType => "org.graylog2.inputs.gelf.udp.GELFUDPInput";

        protected override string InputTitle => "Gelf.Extensions.Logging.Tests.Udp";
    }
}
