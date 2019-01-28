using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public class SingleTelemetryConversionTest : ApplicationInsightsTest
    {
        public SingleTelemetryConversionTest() : base((Func<LogEvent, IFormatProvider, ITelemetry>)ConvertSingle)
        {

        }

        [Fact]
        public void Converer_triggers()
        {
            Logger.Information("test");

            Assert.Equal("converted!", LastSubmittedTraceTelemetry.Message);
        }

        private static ITelemetry ConvertSingle(LogEvent e, IFormatProvider formatProvider)
        {
            var tt = e.ToDefaultTraceTelemetry(formatProvider);

            tt.Message = "converted!";

            return tt;
        }
    }
}
