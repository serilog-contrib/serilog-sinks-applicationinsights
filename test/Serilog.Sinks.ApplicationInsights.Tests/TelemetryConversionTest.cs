using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public class TelemetryConversionTest : ApplicationInsightsTest
    {
        public TelemetryConversionTest() : base(new CustomConverter())
        {

        }

        [Fact]
        public void Converter_triggers()
        {
            Logger.Information("test");

            Assert.Single(SubmittedTelemetry);
        }

        [Fact]
        public void Convert_to_two_traces()
        {
            Logger.Information("two");

            Assert.Equal(2, SubmittedTelemetry.Count(t => t is TraceTelemetry tt && tt.Message == "two"));
        }

        private class CustomConverter : TraceTelemetryConverter
        {
            public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
            {
                IEnumerable<ITelemetry> tt = base.Convert(logEvent, formatProvider);

                if (logEvent.MessageTemplate.Text == "two")
                {
                    yield return tt.First();
                    yield return tt.First();
                }
                else
                {
                    yield return tt.First();
                }
            }
        }
    }
}
