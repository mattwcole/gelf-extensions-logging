using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Gelf.Extensions.Logging;

internal class GelfLoggerOptionsSetup : ConfigureFromConfigurationOptions<GelfLoggerOptions>
{
    public GelfLoggerOptionsSetup(ILoggerProviderConfiguration<GelfLoggerProvider> providerConfiguration)
        : base(providerConfiguration.Configuration)
    {
    }
}