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
            Logger.Information("Data: {MyData}", "This string is \"quoted\"");
            Assert.Equal("Data: This string is \"quoted\"", LastSubmittedTraceTelemetry.Message);
        }

        [Fact]
        public void MessagePropertyQuotesAreNotEscaped()
        {
            Logger.Information("Data: {MyData}", "This string is \"quoted\"");
            Assert.Equal("This string is \"quoted\"", LastSubmittedTraceTelemetry.Properties["MyData"]);
        }
    }
}
