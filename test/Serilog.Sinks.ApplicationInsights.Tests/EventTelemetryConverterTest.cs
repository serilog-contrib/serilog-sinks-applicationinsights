using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public class EventTelemetryConverterTest : ApplicationInsightsTest
    {
        public EventTelemetryConverterTest() : base(new EventTelemetryConverter())
        {
        }

        [Fact]
        public void MessagesAreFormattedWithoutQuotedStrings()
        {
            Logger.Information("Hello, {Name}!", "world");
            Assert.Equal("Hello, world!", LastSubmittedEventTelemetry.Properties["RenderedMessage"]);
        }

        [Fact]
        public void MessageQuotesAreNotEscaped()
        {
            var value = "This string is \"quoted\"";
            Logger.Information("Data: {MyData}", value);

            Assert.Equal($"Data: {value}", LastSubmittedTraceTelemetry.Message);
        }

        [Fact]
        public void MessagePropertyQuotesAreNotEscaped()
        {
            var value = "This string is \"quoted\"";
            Logger.Information("Data: {MyData}", value);

            Assert.Equal($"{value}", LastSubmittedTraceTelemetry.Properties["MyData"]);
        }
    }
}
