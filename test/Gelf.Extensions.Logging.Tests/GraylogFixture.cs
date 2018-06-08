using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public abstract class GraylogFixture : IAsyncLifetime
    {
        public static string GraylogHost = Environment.GetEnvironmentVariable("GRAYLOG_HOST") ?? "localhost";

        private const string GraylogUsername = "admin";
        private const string GraylogPassword = "admin";
        private const int GraylogApiPort = 9000;
        private const int ApiPollInterval = 200;
        private const int ApiPollTimeout = 10000;

        private readonly HttpClientWrapper _httpClient;

        protected GraylogFixture()
        {
            _httpClient = new HttpClientWrapper(
                $"http://{GraylogHost}:{GraylogApiPort}/api/", GraylogUsername, GraylogPassword);
        }

        public abstract int InputPort { get; }

        public abstract string InputType { get; }

        public abstract string InputTitle { get; }

        public async Task InitializeAsync()
        {
            await WaitForGraylogAsync();
            var inputId = await CreateInputAsync();
            await WaitForInputAsync(inputId);
        }

        private Task WaitForGraylogAsync()
        {
            return RepeatUntilAsync(async cancellation =>
            {
                try
                {
                    await _httpClient.GetAsync("system/stats", cancellation);
                    return true;
                }
                catch (HttpRequestException)
                {
                    return false;
                }
            }, retryInterval: 2000, retryTimeout: 60000);
        }

        private async Task<string> CreateInputAsync()
        {
            List<dynamic> existingInputs = (await _httpClient.GetAsync("system/inputs")).inputs;
            var input = existingInputs.SingleOrDefault(i => i.attributes.port == InputPort);
            if (input != null)
            {
                return input.id;
            }

            var newInputRequest = new
            {
                title = InputTitle,
                global = true,
                type = InputType,
                configuration = new
                {
                    bind_address = "0.0.0.0",
                    decompress_size_limit = 8388608,
                    override_source = default(object),
                    port = InputPort,
                    recv_buffer_size = 212992
                }
            };

            var newInputResponse = await _httpClient.PostAsync(newInputRequest, "system/inputs");
            return newInputResponse.id;
        }

        private Task WaitForInputAsync(string inputId)
        {
            return RepeatUntilAsync(async cancellation =>
                await _httpClient.GetAsync($"system/inputstates/{inputId}", cancellation) != null);
        }

        public async Task<List<dynamic>> WaitForMessagesAsync(int count = 1)
        {
            var query = $"test_id:\"{TestContext.TestId}\"";
            var url = $"search/universal/relative?query={WebUtility.UrlEncode(query)}&range=60";

            var messages = new List<dynamic>();

            await RepeatUntilAsync(async cancellation =>
            {
                messages.AddRange((await _httpClient.GetAsync(url, cancellation)).messages);
                return messages.Count == count;
            });

            return messages.Select(m => m.message).ToList();
        }

        public async Task<dynamic> WaitForMessageAsync()
        {
            return (await WaitForMessagesAsync()).Single();
        }

        private static async Task RepeatUntilAsync(Func<CancellationToken, Task<bool>> predicate,
            int retryInterval = ApiPollInterval, int retryTimeout = ApiPollTimeout)
        {
            using (var cts = new CancellationTokenSource(retryTimeout))
            {
                while (!await predicate(cts.Token))
                {
                    await Task.Delay(retryInterval, cts.Token);
                }
            }
        }

        public Task DisposeAsync()
        {
            _httpClient.Dispose();
            return Task.CompletedTask;
        }
    }
}
