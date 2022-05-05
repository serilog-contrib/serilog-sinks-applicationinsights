using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests;

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
    public void MessagesAreFormattedWithoutQuotedStringsWhenDestructuring()
    {
        Logger.Information("Hello, {@Name}", new { Foo = "foo", Bar = 123 });
        Assert.Equal("Hello, {\"Foo\":\"foo\",\"Bar\":123}", LastSubmittedEventTelemetry.Properties["RenderedMessage"]);
    }

    [Fact]
    public void MessageQuotesAreNotEscaped()
    {
        Logger.Information("Data: {MyData}", "This string is \"quoted\"");
        Assert.Equal("Data: This string is \"quoted\"", LastSubmittedEventTelemetry.Properties["RenderedMessage"]);
    }

    [Fact]
    public void DestructuredPropertyIsFormattedCorrectly()
    {
        Logger.Information("Hello, {@MyData}", new { Foo = "foo", Bar = 123 });
        Assert.Equal("{\"Foo\":\"foo\",\"Bar\":123}", LastSubmittedEventTelemetry.Properties["MyData"]);
    }
}
