using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.ExtensionMethods;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            Assert.Equal("converted!", LastSubmittedTraceTelemetry.Message);
        }

        [Fact]
        public void Convert_to_two_traces()
        {
            Logger.Information("two");

            Assert.Equal(2, SubmittedTelemetry.Count(t => t is TraceTelemetry tt && tt.Message == "two"));
        }

        private class CustomConverter : ITelemetryConverter
        {
            public IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
            {
                if (logEvent.MessageTemplate.Text == "two")
                {
                    var tt = logEvent.ToDefaultTraceTelemetry(formatProvider);

                    yield return tt;
                    yield return tt;
                }
                else
                {

                    var tt = logEvent.ToDefaultTraceTelemetry(formatProvider);

                    tt.Message = "converted!";

                    yield return tt;
                }
            }
        }
    }
}
