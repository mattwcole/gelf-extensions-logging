using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class UdpGelfLoggerTests : GelfLoggerTests, IClassFixture<UdpGraylogFixture>
    {
        public UdpGelfLoggerTests(UdpGraylogFixture graylogFixture) : base(graylogFixture)
        {

        }
    }
}
