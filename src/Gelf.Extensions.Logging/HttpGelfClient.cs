using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gelf.Extensions.Logging
{
    public class HttpGelfClient : IGelfClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly GelfLoggerOptions _options;

        public HttpGelfClient(GelfLoggerOptions options)
        {
            _options = options;

            // Setup HTTP client
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(options.Host);
        }

        public async Task SendMessageAsync(GelfMessage message)
        {
            var json = GetMessageString(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("gelf", content);
            result.EnsureSuccessStatusCode();
        }

        private static string GetMessageString(GelfMessage message)
        {
            var messageJson = JObject.FromObject(message);

            foreach (var field in message.AdditionalFields)
            {
                messageJson[$"_{field.Key}"] = field.Value?.ToString();
            }

            var messageString = JsonConvert.SerializeObject(messageJson, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return messageString;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
