using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gelf.Extensions.Logging
{
    public class HttpGelfClient : IGelfClient
    {
        private readonly HttpClient _httpClient;
        
        public HttpGelfClient(GelfLoggerOptions options)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = options.Protocol.ToString().ToLower(),
                Host = options.Host,
                Port = options.Port
            };

            _httpClient = new HttpClient
            {
                BaseAddress = uriBuilder.Uri,
                Timeout = options.HttpTimeout
            };

            if (options.Headers != null && options.Headers.Count > 0)
            {
                foreach (var header in options.Headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        public async Task SendMessageAsync(GelfMessage message)
        {
            var content = new StringContent(message.ToJson(), Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("gelf", content);
            result.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
