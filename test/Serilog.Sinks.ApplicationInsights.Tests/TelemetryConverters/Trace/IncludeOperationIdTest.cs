using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Trace;

public class IncludeOperationIdTest : ApplicationInsightsTest
{
    public IncludeOperationIdTest()
        : base(new TraceTelemetryConverter(true, false, false, false), true, true)
    {
    }

    [Fact]
    public void OperationIdIsSetAsTraceProperty()
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information("Hello, {operationId}!", "foo-operation-id");

        Assert.Equal("foo-operation-id", LastSubmittedTraceTelemetry.Properties["operationId"]);
    }
}
