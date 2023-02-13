using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gelf.Extensions.Logging;

[ProviderAlias("GELF")]
public class GelfLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IOptionsMonitor<GelfLoggerOptions> _options;
    private readonly ConcurrentDictionary<string, GelfLogger> _loggers;
    private readonly IDisposable _optionsReloadToken;

    private IGelfClient? _gelfClient;
    private GelfMessageProcessor? _messageProcessor;
    private IExternalScopeProvider? _scopeProvider;

    public GelfLoggerProvider(IOptionsMonitor<GelfLoggerOptions> options)
    {
        _options = options;
        _loggers = new ConcurrentDictionary<string, GelfLogger>();

        LoadLoggerOptions(options.CurrentValue);

        var onOptionsChanged = Debouncer.Debounce<GelfLoggerOptions>(LoadLoggerOptions, TimeSpan.FromSeconds(1));
        _optionsReloadToken = options.OnChange(onOptionsChanged);
    }

    public ILogger CreateLogger(string name)
    {
        return _loggers.GetOrAdd(name, newName => new GelfLogger(
            newName, _messageProcessor!, _options.CurrentValue)
        {
            ScopeProvider = _scopeProvider
        });
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
        foreach (var logger in _loggers)
        {
            logger.Value.ScopeProvider = _scopeProvider;
        }
    }

    private void LoadLoggerOptions(GelfLoggerOptions options)
    {
        if (string.IsNullOrEmpty(options.Host))
        {
            throw new ArgumentException("GELF host is required.", nameof(options));
        }

        if (string.IsNullOrEmpty(options.LogSource))
        {
            throw new ArgumentException("GELF log source is required.", nameof(options));
        }

        var gelfClient = CreateGelfClient(options);

        if (_messageProcessor == null)
        {
            _messageProcessor = new GelfMessageProcessor(gelfClient);
            _messageProcessor.Start();
        }
        else
        {
            _messageProcessor.GelfClient = gelfClient;
            _gelfClient?.Dispose();
        }

        _gelfClient = gelfClient;

        foreach (var logger in _loggers)
        {
            logger.Value.Options = options;
        }
    }

    private static IGelfClient CreateGelfClient(GelfLoggerOptions options)
    {
        return options.Protocol switch
        {
            GelfProtocol.Udp => new UdpGelfClient(options),
            GelfProtocol.Tcp => new TcpGelfClient(options),
            GelfProtocol.Http => new HttpGelfClient(options),
            GelfProtocol.Https => new HttpGelfClient(options),
            _ => throw new ArgumentException("Unknown protocol.", nameof(options))
        };
    }

    public void Dispose()
    {
        _messageProcessor?.Stop();
        _gelfClient?.Dispose();
        _optionsReloadToken.Dispose();
    }
}