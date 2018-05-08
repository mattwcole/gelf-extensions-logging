using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class GelfLoggerTests : IClassFixture<GraylogFixture>, IDisposable
    {
        private readonly GraylogFixture _graylogFixture;
        private readonly LoggerFixture _loggerFixture;
        private readonly Faker _faker;

        public GelfLoggerTests(GraylogFixture graylogFixture)
        {
            _graylogFixture = graylogFixture;
            _loggerFixture = new LoggerFixture();
            _faker = new Faker();
        }

        [Theory]
        [InlineData(LogLevel.Critical, 2)]
        [InlineData(LogLevel.Error, 3)]
        [InlineData(LogLevel.Warning, 4)]
        [InlineData(LogLevel.Information, 6)]
        [InlineData(LogLevel.Debug, 7)]
        [InlineData(LogLevel.Trace, 7)]
        public async Task Sends_message_to_Graylog(LogLevel logLevel, int expectedLevel)
        {
            var messageText = _faker.Lorem.Sentence();
            var sut = _loggerFixture.CreateLogger<GelfLoggerTests>();

            sut.Log(logLevel, new EventId(), (object) null, null, (s, e) => messageText);

            var message = await _graylogFixture.WaitForMessageAsync();

            Assert.NotEmpty(message._id);
            Assert.Equal(_loggerFixture.LoggerOptions.LogSource, message.source);
            Assert.Equal(messageText, message.message);
            Assert.Equal(expectedLevel, message.level);
            Assert.Equal(typeof(GelfLoggerTests).FullName, message.logger);
            Assert.Throws<RuntimeBinderException>(() => message.exception);
        }

        [Fact]
        public async Task Includes_exceptions_on_messages()
        {
            var exception = new Exception("Something went wrong!");
            var sut = _loggerFixture.CreateLogger<GelfLoggerTests>();

            sut.LogError(new EventId(), exception, _faker.Lorem.Sentence());

            var message = await _graylogFixture.WaitForMessageAsync();
            
            Assert.Equal(exception.ToString(), message.exception);
        }

        [Theory]
        [InlineData(50, 100)]
        [InlineData(200, 100)]
        [InlineData(300, 300)]
        [InlineData(23000, 25000)]
        [InlineData(12000, 10000)]
        public async Task Sends_message_with_and_without_compression(int compressionThreshold, int messageSize)
        {
            var options = _loggerFixture.LoggerOptions;
            options.CompressionThreshold = compressionThreshold;
            var messageText = new string('*', messageSize);

            using (var loggerFactory = _loggerFixture.CreateLoggerFactory(options))
            {
                var sut = loggerFactory.CreateLogger(nameof(GelfLoggerTests));
                sut.LogInformation(messageText);

                var message = await _graylogFixture.WaitForMessageAsync();

                Assert.NotEmpty(message._id);
                Assert.Equal(options.LogSource, message.source);
                Assert.Equal(messageText, message.message);
                Assert.Equal(6, message.level);
            }
        }

        [Fact]
        public async Task Sends_message_with_additional_fields_from_options()
        {
            var options = _loggerFixture.LoggerOptions;
            options.AdditionalFields["foo"] = "foo";
            options.AdditionalFields["bar"] = "bar";
            var messageText = _faker.Lorem.Sentence();

            using (var loggerFactory = _loggerFixture.CreateLoggerFactory(options))
            {
                var sut = loggerFactory.CreateLogger(nameof(GelfLoggerTests));
                sut.LogInformation(messageText);

                var message = await _graylogFixture.WaitForMessageAsync();

                Assert.Equal("foo", message.foo);
                Assert.Equal("bar", message.bar);
            }
        }

        [Fact]
        public async Task Sends_message_with_additional_fields_from_scope()
        {
            var messageText = _faker.Lorem.Sentence();

            var sut = _loggerFixture.CreateLogger<GelfLoggerTests>();
            using (sut.BeginScope(new Dictionary<string, object>
            {
                ["baz"] = "baz",
                ["qux"] = "qux"
            }))
            {
                sut.LogCritical(messageText);
            }

            var message = await _graylogFixture.WaitForMessageAsync();

            Assert.Equal("baz", message.baz);
            Assert.Equal("qux", message.qux);
        }

        [Fact]
        public async Task Sends_message_with_additional_fields_from_structured_log()
        {
            var sut = _loggerFixture.CreateLogger<GelfLoggerTests>();
            sut.LogDebug("Structured log line with {first_value} and {second_value}", "foo", "bar");

            var message = await _graylogFixture.WaitForMessageAsync();

            Assert.Equal("Structured log line with foo and bar", message.message);
            Assert.Equal("foo", message.first_value);
            Assert.Equal("bar", message.second_value);
        }

        [Fact]
        public async Task Ignores_null_values_in_additional_fields()
        {
            var options = _loggerFixture.LoggerOptions;
            options.AdditionalFields["foo"] = null;

            using (var loggerFactory = _loggerFixture.CreateLoggerFactory(options))
            {
                var sut = loggerFactory.CreateLogger(nameof(GelfLoggerTests));

                using (sut.BeginScope(("bar", (string) null)))
                using (sut.BeginScope(new Dictionary<string, object>
                {
                    ["baz"] = null
                }))
                {
                    sut.LogInformation(_faker.Lorem.Sentence());
                }

                var message = await _graylogFixture.WaitForMessageAsync();

                Assert.Throws<RuntimeBinderException>(() => message.foo);
                Assert.Throws<RuntimeBinderException>(() => message.bar);
                Assert.Throws<RuntimeBinderException>(() => message.baz);
            }
        }

        public void Dispose()
        {
            _loggerFixture.Dispose();
        }
    }
}
