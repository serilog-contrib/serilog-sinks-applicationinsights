using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public class MultipleTelemetryConversionTest : ApplicationInsightsTest
    {
        public MultipleTelemetryConversionTest() : base(ConvertMultiple)
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

        private static IEnumerable<ITelemetry> ConvertMultiple(LogEvent e, IFormatProvider formatProvider, TelemetryClient telemetryClient)
        {
            if (e.MessageTemplate.Text == "two")
            {
                var tt = e.ToDefaultTraceTelemetry(formatProvider);

                return new[] { tt, tt };
            }
            else
            {

                var tt = e.ToDefaultTraceTelemetry(formatProvider);

                tt.Message = "converted!";

                return new[] { tt };
            }
        }
    }
}
