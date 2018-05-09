﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gelf.Extensions.Logging
{
    // http://docs.graylog.org/en/2.4/pages/gelf.html#gelf-payload-specification
    public class GelfMessage
    {
        [JsonProperty("version")]
        public string Version { get; } = "1.1";

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("short_message")]
        public string ShortMessage { get; set; }

        [JsonProperty("timestamp")]
        public double Timestamp { get; set; }

        [JsonProperty("level")]
        public SyslogSeverity Level { get; set; }

        [JsonProperty("eventid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int EventId { get; set; }

        [JsonProperty("eventname", NullValueHandling = NullValueHandling.Ignore)]
        public string EventName { get; set; }

        [JsonIgnore]
        public IEnumerable<KeyValuePair<string, object>> AdditionalFields { get; set; }
    }
}
