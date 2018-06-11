﻿using System;
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
        }

        public async Task SendMessageAsync(GelfMessage message)
        {
            var content = new StringContent(message.ToJson(), Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("gelf", content).ConfigureAwait(false);
            result.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
