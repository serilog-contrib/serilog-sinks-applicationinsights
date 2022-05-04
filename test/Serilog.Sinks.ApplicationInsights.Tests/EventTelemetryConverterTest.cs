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
    }
}
