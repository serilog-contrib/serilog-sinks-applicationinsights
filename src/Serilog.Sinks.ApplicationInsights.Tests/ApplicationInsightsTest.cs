using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public abstract class ApplicationInsightsTest
    {
        private readonly UnitTestTelemetryChannel _channel;

        public ApplicationInsightsTest()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ApplicationInsightsTraces("")
                .MinimumLevel.Debug()
                .CreateLogger();

            var tc = TelemetryConfiguration.Active;

            tc.TelemetryChannel = _channel = new UnitTestTelemetryChannel();
        }

        protected List<ITelemetry> SubmittedTelemetry => _channel.SubmittedTelemetry;

        protected ITelemetry LastSubmittedTelemetry => _channel.SubmittedTelemetry.LastOrDefault();

        protected TraceTelemetry LastSubmittedTraceTelemetry => 
            _channel.SubmittedTelemetry
                .Where(t => t is TraceTelemetry)
                .Select(t => (TraceTelemetry)t)
                .LastOrDefault();
    }
}
