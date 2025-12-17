using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Event;

public class IncludeOperationIdTest : ApplicationInsightsTest
{
    public IncludeOperationIdTest()
        : base(new EventTelemetryConverter(true, false, false, false), true, true)
    {
    }

    [Fact]
    public void OperationIdIsSetAsTraceProperty()
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information("Hello, {operationId}!", "foo-operation-id");

        Assert.Equal("foo-operation-id", LastSubmittedEventTelemetry.Properties["operationId"]);
    }
}
