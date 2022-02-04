using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gelf.Extensions.Logging
{
    public class TcpGelfClient : IGelfClient
    {
        private readonly ReaderWriterLockSlim _lockSlim = new();
        private readonly GelfLoggerOptions _options;
        private TcpClient? _client;
        private Stream? _stream;

        public TcpGelfClient(GelfLoggerOptions options)
        {
            _options = options;
        }

        public void Dispose()
        {
            _lockSlim.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }

        public async Task SendMessageAsync(GelfMessage message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message.ToJson() + '\0');
            try
            {
                var stream = GetStream();
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            }
            catch (SocketException)
            {
                if (_options.ThrowTcpExceptions)
                    throw;
            }
        }

        private Stream GetStream()
        {
            _lockSlim.EnterUpgradeableReadLock();
            try
            {
                if (_client?.Connected == true && _stream != null)
                    return _stream;

                _lockSlim.EnterWriteLock();
                try
                {
                    _client = new TcpClient(_options.Host!, _options.Port) {SendTimeout = _options.TcpTimeoutMs};
                    _stream = _client.GetStream();
                    return _stream;
                }
                finally
                {
                    _lockSlim.ExitWriteLock();
                }
            }
            finally
            {
                _lockSlim.ExitUpgradeableReadLock();
            }
        }
    }
}
