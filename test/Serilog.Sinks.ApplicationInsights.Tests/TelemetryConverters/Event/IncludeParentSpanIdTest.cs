using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Event;

public class IncludeSpanIdTest : ApplicationInsightsTest
{
    public IncludeSpanIdTest()
        : base(new EventTelemetryConverter(false, true, false, false, true), true, true)
    {
    }

    [Theory]
    [InlineData("Hello, {parentSpanId}!", "parentSpanId", "foo-parent-span-id")]
    [InlineData("Hello, {ParentSpanId}!", "ParentSpanId", "bar-parent-span-id")]
    public void ParentSpanIdIsSetAsTraceProperty(string pattern, string parentSpanIdKey, string expectedParentSpanId)
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information(pattern, expectedParentSpanId);

        Assert.Equal(expectedParentSpanId, LastSubmittedEventTelemetry.Properties[parentSpanIdKey]);
    }
}
