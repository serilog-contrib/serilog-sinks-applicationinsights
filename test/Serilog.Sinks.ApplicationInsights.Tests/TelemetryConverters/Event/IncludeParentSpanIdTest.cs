using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Event;

public class IncludeSpanIdTest : ApplicationInsightsTest
{
    public IncludeSpanIdTest()
        : base(new EventTelemetryConverter(false, true, false, false), true, true)
    {
    }

    [Fact]
    public void OperationIdIsSetAsTraceProperty()
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

    Logger.Information("Hello, {ParentSpanId}!", "foo-parent-span-id");

        Assert.Equal("foo-parent-span-id", LastSubmittedEventTelemetry.Properties["ParentSpanId"]);
    }
}
