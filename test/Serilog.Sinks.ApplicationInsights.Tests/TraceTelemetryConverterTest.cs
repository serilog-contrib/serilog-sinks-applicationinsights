using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests;

public class TraceTelemetryConverterTest : ApplicationInsightsTest
{
    public TraceTelemetryConverterTest() : base(new TraceTelemetryConverter())
    {
    }

    [Fact]
    public void MessagesAreFormattedWithoutQuotedStrings()
    {
        Logger.Information("Hello, {Name}!", "world");
        Assert.Equal("Hello, world!", LastSubmittedTraceTelemetry.Message);
    }

    [Fact]
    public void MessagesAreFormattedWithoutQuotedStringsWhenDestructuring()
    {
        Logger.Information("Hello, {@Name}", new { Foo = "foo", Bar = 123 });
        Assert.Equal("Hello, { \"Foo\": \"foo\", \"Bar\": 123 }", LastSubmittedTraceTelemetry.Message);
    }

    [Fact]
    public void MessageQuotesAreNotEscaped()
    {
        Logger.Information("Data: {MyData}", "This string is \"quoted\"");
        Assert.Equal("Data: This string is \"quoted\"", LastSubmittedTraceTelemetry.Message);
    }
}
