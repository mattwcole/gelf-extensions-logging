namespace Gelf.Extensions.Logging.Tests.Fixtures
{
    public class UdpGraylogFixture : GraylogFixture
    {
        public override int InputPort => 12201;

        public override string InputType => "org.graylog2.inputs.gelf.udp.GELFUDPInput";

        public override string InputTitle => "Gelf.Extensions.Logging.Tests.Udp";
    }
}
