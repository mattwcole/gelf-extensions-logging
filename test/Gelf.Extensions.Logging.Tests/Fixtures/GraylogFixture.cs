using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Gelf.Extensions.Logging.Tests.Fixtures
{
    public abstract class GraylogFixture : IAsyncLifetime
    {
        private const string GraylogUsername = "admin";
        private const string GraylogPassword = "admin";
        private const int GraylogApiPort = 9000;
        private const int ApiPollInterval = 200;
        private const int ApiPollTimeout = 10000;

        private readonly IMessageSink _messageSink;
        private readonly HttpClientWrapper _httpClient;

        protected GraylogFixture(IMessageSink messageSink)
        {
            _messageSink = messageSink;
            _httpClient = new HttpClientWrapper(
                $"http://{Host}:{GraylogApiPort}/api/", GraylogUsername, GraylogPassword);
        }

        public string Host => Environment.GetEnvironmentVariable("GRAYLOG_HOST") ?? "localhost";

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
                    WriteLine("Waiting for Graylog server...");

                    var system = await _httpClient.GetAsync("system", cancellation);
                    TimeSpan uptime = DateTime.UtcNow - DateTime.Parse(system.started_at.ToString());

                    WriteLine($"Graylog system details:{Environment.NewLine}{JsonConvert.SerializeObject(system)}");
                    WriteLine($"Graylog server has been up for {uptime.TotalSeconds} seconds");

                    return system.lifecycle == "running";
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
            return RepeatUntilAsync(async delegate(CancellationToken cancellation)
            {
                WriteLine($"Waiting for Graylog input {inputId}...");

                var inputState = await _httpClient.GetAsync($"system/inputstates/{inputId}", cancellation);

                if (inputState != null)
                {
                    WriteLine($"Graylog input details:{Environment.NewLine}{JsonConvert.SerializeObject(inputState)}");
                }

                return inputState?.state == "RUNNING";
            }, retryInterval: 2000);
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

        protected void WriteLine(string message)
        {
            _messageSink.OnMessage(new DiagnosticMessage(message));
        }

        public Task DisposeAsync()
        {
            _httpClient.Dispose();
            return Task.CompletedTask;
        }
    }
}
