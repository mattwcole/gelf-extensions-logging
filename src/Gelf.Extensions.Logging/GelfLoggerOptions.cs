namespace Gelf.Extensions.Logging
{
    public class GelfLoggerOptions
    {
        public string GelfHost { get; set; }

        public int GelfPort { get; set; } = 12201;

        /// <summary>
        /// Log source name mapped to the GELF host field.
        /// </summary>
        public string AppHost { get; set; }

        /// <summary>
        /// Enable GZip message compression.
        /// </summary>
        public bool Compress { get; set; } = true;

        /// <summary>
        /// The message size in bytes under which messages will not be compressed.
        /// </summary>
        public int CompressionThreshold { get; set; } = 500;
    }
}
