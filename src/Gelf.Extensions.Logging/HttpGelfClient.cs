using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gelf.Extensions.Logging
{
    public class HttpGelfClient : IGelfClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        
        public HttpGelfClient(GelfLoggerOptions options)
        {
            _httpClient = new HttpClient {BaseAddress = new Uri(options.Host)};
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
