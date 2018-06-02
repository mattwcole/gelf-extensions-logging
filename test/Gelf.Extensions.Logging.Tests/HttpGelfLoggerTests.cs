using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class HttpGelfLoggerTests : GelfLoggerTests, IClassFixture<HttpGraylogFixture>
    {
        public HttpGelfLoggerTests(HttpGraylogFixture graylogFixture) : base(graylogFixture)
        {

        }
    }
}
