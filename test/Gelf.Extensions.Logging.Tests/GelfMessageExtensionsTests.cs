using System;
using System.Collections.Generic;
using Xunit;

namespace Gelf.Extensions.Logging.Tests;

public class GelfMessageExtensionsTests
{
    [Fact]
    public void Serialises_to_JSON_string_with_correct_settings()
    {
        var message = new GelfMessage
        {
            Level = SyslogSeverity.Emergency,
            AdditionalFields = new Dictionary<string, object>()
        };

        var messageJson = message.ToJson();

        Assert.Contains("version", messageJson);
        Assert.DoesNotContain("Emergency", messageJson);
        Assert.DoesNotContain(Environment.NewLine, messageJson);
        Assert.DoesNotContain("null", messageJson);
    }
}