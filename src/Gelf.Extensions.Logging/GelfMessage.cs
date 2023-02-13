using System;
using System.Collections.Generic;

namespace Gelf.Extensions.Logging;

// https://docs.graylog.org/en/3.1/pages/gelf.html#gelf-payload-specification
public class GelfMessage
{
    public string Version { get; } = "1.1";

    public string? Host { get; set; }

    public string? ShortMessage { get; set; }

    public double Timestamp { get; set; }

    public SyslogSeverity Level { get; set; }

    public IReadOnlyCollection<KeyValuePair<string, object?>> AdditionalFields { get; set; } =
        Array.Empty<KeyValuePair<string, object?>>();
}