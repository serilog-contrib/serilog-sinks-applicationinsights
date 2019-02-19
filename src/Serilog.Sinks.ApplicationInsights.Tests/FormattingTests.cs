using Microsoft.ApplicationInsights.Channel;
using Serilog.Context;
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

        [Fact]
        public void Message_properies_include_log_context()
        {
            using (LogContext.PushProperty("custom1", "value1"))
            {
                Logger.Information("test context");

                Assert.True(LastSubmittedTraceTelemetry.Properties.TryGetValue("custom1", out string value1) && value1 == "value1");
            }
        }

        [Fact]
        public void Json_parameter_is_compact()
        {
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            var numbers = new int[] { 1, 2, 3, 4 };

            Logger.Information("Processed {@Position} in {Elapsed:000} ms., str {str}, numbers: {numbers}", position, elapsedMs, "test", numbers);

            Assert.Equal("34", LastSubmittedTraceTelemetry.Properties["Elapsed"]);
            Assert.Equal("{\"Latitude\":25,\"Longitude\":134}", LastSubmittedTraceTelemetry.Properties["Position"]);
            Assert.Equal("[1,2,3,4]", LastSubmittedTraceTelemetry.Properties["numbers"]);
        }
    }
}
