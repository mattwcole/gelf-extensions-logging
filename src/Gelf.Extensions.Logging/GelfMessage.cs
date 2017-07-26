using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gelf.Extensions.Logging
{
    // http://docs.graylog.org/en/2.2/pages/gelf.html#gelf-payload-specification
    public class GelfMessage
    {
        [JsonProperty("version")]
        public string Version { get; } = "1.1";

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("short_message")]
        public string ShortMessage { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("level")]
        public SyslogSeverity Level { get; set; }

        [JsonIgnore]
        public IEnumerable<KeyValuePair<string, string>> AdditionalFields { get; set; }
    }
}
