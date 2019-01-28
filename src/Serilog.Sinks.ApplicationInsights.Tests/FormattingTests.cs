using Microsoft.ApplicationInsights.Channel;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public class FormattingTests : ApplicationInsightsTest
    {
        [Fact]
        public void Log_level_is_not_in_trace_custom_property()
        {
            Logger.Information("test");

            Assert.False(LastSubmittedTraceTelemetry.Properties.ContainsKey("LogLevel"));
        }

        [Fact]
        public void Message_template_is_not_in_trace_custom_property()
        {
            Logger.Information("test");

            Assert.False(LastSubmittedTraceTelemetry.Properties.ContainsKey("MessageTemplate"));
        }
    }
}
