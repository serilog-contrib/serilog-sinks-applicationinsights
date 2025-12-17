using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Trace;

public class IncludeSpanIdTest : ApplicationInsightsTest
{
    public IncludeSpanIdTest()
        : base(new TraceTelemetryConverter(false, true, false, false), true, true)
    {
    }

    [Fact]
    public void OperationIdIsSetAsTraceProperty()
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information("Hello, {ParentSpanId}!", "foo-parent-span-id");

        Assert.Equal("foo-parent-span-id", LastSubmittedTraceTelemetry.Properties["ParentSpanId"]);
    }
}
