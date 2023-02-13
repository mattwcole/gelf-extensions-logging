using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gelf.Extensions.Logging;

public class TcpGelfClient : IGelfClient
{
    private readonly GelfLoggerOptions _options;
    private TcpClient? _client;
    private Stream? _stream;

    public TcpGelfClient(GelfLoggerOptions options)
    {
        _options = options;
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
    }

    public async Task SendMessageAsync(GelfMessage message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message.ToJson() + '\0');
        try
        {
            var stream = GetStream(false);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(true);
            await stream.FlushAsync().ConfigureAwait(true);
        }
        catch (IOException)
        {
            // Retry once on IOException (in case of OS aborted connections)
            var stream = GetStream(true);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(true);
            await stream.FlushAsync().ConfigureAwait(true);
        }
    }

    private Stream GetStream(bool recreate)
    {
        if (recreate || _client == null || _stream == null || !_client.Connected)
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch
            {
                // Ignore any error during the closing of the client or stream
            }

            _client = new TcpClient(_options.Host!, _options.Port);
            _stream = _client.GetStream();
        }

        return _stream;
    }
}