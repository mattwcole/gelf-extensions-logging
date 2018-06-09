namespace Gelf.Extensions.Logging.Tests
{
    public class HttpGraylogFixture : GraylogFixture
    {
        public override int InputPort => 12202;

        public override string InputType => "org.graylog2.inputs.gelf.http.GELFHttpInput";

        public override string InputTitle => "Gelf.Extensions.Logging.Tests.Http";
    }
}
