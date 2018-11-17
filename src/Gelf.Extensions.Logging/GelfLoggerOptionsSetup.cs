#if NETSTANDARD2_0

using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Gelf.Extensions.Logging
{
    public class GelfLoggerOptionsSetup : ConfigureFromConfigurationOptions<GelfLoggerOptions>
    {
        public GelfLoggerOptionsSetup(ILoggerProviderConfiguration<GelfLoggerProvider> providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}

#endif
