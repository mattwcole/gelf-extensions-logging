using System;
using System.Diagnostics;
using Bogus;
using Essential.Diagnostics;
using Gelf.Extensions.Logging.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable InconsistentLogPropertyNaming

namespace Gelf.Extensions.Logging.Tests
{
    public class TraceErrorTests
    {
        private readonly LoggerFixture _loggerFixture;
        private readonly Faker _faker;
        private readonly InMemoryTraceListener _traceListener;

        public TraceErrorTests()
        {
            _loggerFixture = new LoggerFixture(new GelfLoggerOptions
            {
                Host = GraylogFixture.Host,
                LogSource = nameof(TraceErrorTests)
            });

            _faker = new Faker();
            _traceListener = new InMemoryTraceListener();

            Trace.Listeners.Clear();
            Trace.Listeners.Add(_traceListener);
        }

        [Theory]
        [InlineData("exception")]
        [InlineData("invalid field name")]
        public void Writes_trace_event_when_invalid_additional_field_skipped(string invalidFieldName)
        {
            var logger = _loggerFixture.CreateLogger<TraceErrorTests>();

            using var scope = logger.BeginScope((invalidFieldName, "value"));
            logger.LogInformation(_faker.Lorem.Sentence());
            var traceEvents = _traceListener.GetEvents();

            Assert.Single(traceEvents);
            Assert.Contains("invalid key", traceEvents[0].Message);
            Assert.Contains(invalidFieldName, traceEvents[0].Message);
        }

        [Fact]
        public void Writes_trace_event_when_unhandled_exception_in_GELF_client()
        {
            using (var loggerFixture = new LoggerFixture(new GelfLoggerOptions
            {
                Host = "invalid.",
                LogSource = nameof(TraceErrorTests),
                Protocol = GelfProtocol.Https,
                HttpTimeout = TimeSpan.FromSeconds(0.1)
            }))
            {
                var logger = loggerFixture.CreateLogger<TraceErrorTests>();

                logger.LogInformation(_faker.Lorem.Sentence());
            }

            var traceEvents = _traceListener.GetEvents();

            Assert.Single(traceEvents);
            Assert.Contains("Unhandled exception", traceEvents[0].Message);
        }
    }
}
