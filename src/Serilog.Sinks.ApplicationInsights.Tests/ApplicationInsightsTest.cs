using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public abstract class ApplicationInsightsTest
    {
        private readonly UnitTestTelemetryChannel _channel;

        protected ApplicationInsightsTest(Func<LogEvent, IFormatProvider, ITelemetry> conversion)
        {
            var tc = new TelemetryConfiguration("", _channel = new UnitTestTelemetryChannel());

            Logger = new LoggerConfiguration()
                .WriteTo.ApplicationInsights(tc, conversion)
                .MinimumLevel.Debug()
                .CreateLogger();

        }

        protected ApplicationInsightsTest(Func<LogEvent, IFormatProvider, IEnumerable<ITelemetry>> conversion)
        {
            var tc = new TelemetryConfiguration("", _channel = new UnitTestTelemetryChannel());

            Logger = new LoggerConfiguration()
                .WriteTo.ApplicationInsights(tc, conversion)
                .MinimumLevel.Debug()
                .CreateLogger();
        }

        protected ApplicationInsightsTest()
        {
            var tc = new TelemetryConfiguration("", _channel = new UnitTestTelemetryChannel());

            Logger = new LoggerConfiguration()
                .WriteTo.ApplicationInsightsTraces(tc)
                .MinimumLevel.Debug()
                .CreateLogger();
        }

        protected ILogger Logger { get; private set; }

        protected List<ITelemetry> SubmittedTelemetry => _channel.SubmittedTelemetry;

        protected ITelemetry LastSubmittedTelemetry => _channel.SubmittedTelemetry.LastOrDefault();

        protected TraceTelemetry LastSubmittedTraceTelemetry => 
            _channel.SubmittedTelemetry
                .Where(t => t is TraceTelemetry)
                .Select(t => (TraceTelemetry)t)
                .LastOrDefault();

    }
}
