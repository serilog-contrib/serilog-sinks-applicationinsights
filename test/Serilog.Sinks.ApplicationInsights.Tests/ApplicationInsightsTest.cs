using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public abstract class ApplicationInsightsTest
    {
        private readonly UnitTestTelemetryChannel _channel;

        protected ApplicationInsightsTest(ITelemetryConverter converter = null)
        {
            var tc = new TelemetryConfiguration("", _channel = new UnitTestTelemetryChannel());

            Logger = new LoggerConfiguration()
                .WriteTo.ApplicationInsights(tc, converter ?? TelemetryConverter.Traces)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
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
