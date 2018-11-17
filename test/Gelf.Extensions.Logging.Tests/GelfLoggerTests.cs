using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using Gelf.Extensions.Logging.Tests.Fixtures;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public abstract class GelfLoggerTests : IDisposable
    {
        protected readonly GraylogFixture GraylogFixture;
        protected readonly LoggerFixture LoggerFixture;
        protected readonly Faker Faker;

        protected GelfLoggerTests(GraylogFixture graylogFixture, LoggerFixture loggerFixture)
        {
            GraylogFixture = graylogFixture;
            LoggerFixture = loggerFixture;
            Faker = new Faker();
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
            var messageText = Faker.Lorem.Sentence();
            var sut = LoggerFixture.CreateLogger<GelfLoggerTests>();

            sut.Log(logLevel, new EventId(), (object) null, null, (s, e) => messageText);

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.NotEmpty(message._id);
            Assert.Equal(LoggerFixture.LoggerOptions.LogSource, message.source);
            Assert.Equal(messageText, message.message);
            Assert.Equal(expectedLevel, message.level);
            Assert.Equal(typeof(GelfLoggerTests).FullName, message.logger);
            Assert.Throws<RuntimeBinderException>(() => message.exception);
        }

        [Fact]
        public async Task Includes_exceptions_on_messages()
        {
            var messageText = Faker.Lorem.Sentence();
            var exception = new Exception("Something went wrong!");
            var sut = LoggerFixture.CreateLogger<GelfLoggerTests>();

            sut.LogError(new EventId(), exception, messageText);

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.Equal(messageText, message.message);
            Assert.Equal(exception.ToString(), message.exception);
        }

        [Fact]
        public async Task Includes_event_IDs_on_messages()
        {
            var messageText = Faker.Lorem.Sentence();

            var sut = LoggerFixture.CreateLogger<GelfLoggerTests>();
            sut.LogInformation(new EventId(197, "foo"), messageText);

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.Equal(messageText, message.message);
            Assert.Equal(197, message.event_id);
            Assert.Equal("foo", message.event_name);
        }

        [Fact]
        public async Task Sends_message_with_additional_fields_from_options()
        {
            var options = LoggerFixture.LoggerOptions;
            options.AdditionalFields["foo"] = "foo";
            options.AdditionalFields["bar"] = "bar";
            options.AdditionalFields["quux"] = 123;
            var messageText = Faker.Lorem.Sentence();

            using (var loggerFactory = LoggerFixture.CreateLoggerFactory(options))
            {
                var sut = loggerFactory.CreateLogger(nameof(GelfLoggerTests));
                sut.LogInformation(messageText);

                var message = await GraylogFixture.WaitForMessageAsync();

                Assert.Equal("foo", message.foo);
                Assert.Equal("bar", message.bar);
                Assert.Equal(123, message.quux);
            }
        }

        [Fact]
        public async Task Sends_message_with_additional_fields_from_scope()
        {
            var messageText = Faker.Lorem.Sentence();
            var sut = LoggerFixture.CreateLogger<GelfLoggerTests>();
            using (sut.BeginScope(new Dictionary<string, object>
            {
                ["baz"] = "baz",
                ["qux"] = "qux",
                ["quux"] = 123
            }))
            {
                using (sut.BeginScope(("quuz", 456.5)))
                {
                    sut.LogCritical(messageText);
                }
            }

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.Equal("baz", message.baz);
            Assert.Equal("qux", message.qux);
            Assert.Equal(123, message.quux);
            Assert.Equal(456.5, message.quuz);
        }

        [Fact]
        public async Task Sends_message_without_additional_fields_from_scope_when_scope_is_not_included()
        {
            var options = LoggerFixture.LoggerOptions;
            options.IncludeScopes = false;
            options.AdditionalFields.Add("test_id", TestContext.TestId);

            using (var loggerFactory = LoggerFixture.CreateLoggerFactory(options))
            {
                var sut = loggerFactory.CreateLogger(nameof(GelfLoggerTests));
                using (sut.BeginScope(("foo", "bar")))
                {
                    sut.LogInformation(Faker.Lorem.Sentence());
                }

                var message = await GraylogFixture.WaitForMessageAsync();

                Assert.Throws<RuntimeBinderException>(() => message.foo);
            }
        }

        [Fact]
        public async Task Uses_inner_scope_fields_when_keys_duplicated_in_multiple_scopes()
        {
            var sut = LoggerFixture.CreateLogger<GelfLoggerTests>();
            using (sut.BeginScope(("foo", "outer")))
            {
                using (sut.BeginScope(("foo", "inner")))
                {
                    sut.LogCritical(Faker.Lorem.Sentence());
                }
            }

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.Equal("inner", message.foo);
        }

        [Fact]
        public async Task Sends_message_with_additional_fields_from_structured_log()
        {
            var sut = LoggerFixture.CreateLogger<GelfLoggerTests>();
            sut.LogDebug("Structured log line with {first_value} and {second_value}", "foo", 123);

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.Equal("Structured log line with foo and 123", message.message);
            Assert.Equal("foo", message.first_value);
            Assert.Equal(123, message.second_value);
        }

        [Fact]
        public async Task Uses_structured_log_fields_when_keys_duplicated_in_scope()
        {
            var sut = LoggerFixture.CreateLogger<GelfLoggerTests>();
            using (sut.BeginScope(("foo", "scope")))
            {
                sut.LogDebug("Structured log line with {foo}", "structured");
            }

            var message = await GraylogFixture.WaitForMessageAsync();

            Assert.Equal("structured", message.foo);
        }

        [Fact]
        public async Task Ignores_null_values_in_additional_fields()
        {
            var options = LoggerFixture.LoggerOptions;
            options.AdditionalFields["foo"] = null;

            using (var loggerFactory = LoggerFixture.CreateLoggerFactory(options))
            {
                var sut = loggerFactory.CreateLogger(nameof(GelfLoggerTests));

                using (sut.BeginScope(("bar", (string) null)))
                using (sut.BeginScope(new Dictionary<string, object>
                {
                    ["baz"] = null
                }))
                {
                    sut.LogInformation(Faker.Lorem.Sentence());
                }

                var message = await GraylogFixture.WaitForMessageAsync();

                Assert.Throws<RuntimeBinderException>(() => message.foo);
                Assert.Throws<RuntimeBinderException>(() => message.bar);
                Assert.Throws<RuntimeBinderException>(() => message.baz);
            }
        }

        public void Dispose()
        {
            LoggerFixture.Dispose();
        }
    }
}
