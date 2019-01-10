using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class GelfMessageExtensionsTests
    {
        [Fact]
        public void Serialises_to_JSON_string_with_correct_settings()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters =
                {
                    new StringEnumConverter()
                }
            };

            var message = new GelfMessage
            {
                Level = SyslogSeverity.Emergency,
                AdditionalFields = new Dictionary<string, object>()
            };

            var messageJson = message.ToJson();

            Assert.DoesNotContain("Emergency", messageJson);
            Assert.DoesNotContain(Environment.NewLine, messageJson);
            Assert.DoesNotContain("null", messageJson);
        }
    }
}
