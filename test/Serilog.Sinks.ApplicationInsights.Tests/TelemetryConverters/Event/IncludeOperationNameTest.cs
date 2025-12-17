using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Event;

public class IncludeOperationNameTest : ApplicationInsightsTest
{
    public IncludeOperationNameTest()
        : base(new EventTelemetryConverter(false, false, true, false), true, true)
    {
    }

    [Fact]
    public void OperationIdIsSetAsTraceProperty()
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information("Hello, {OperationName}!", "foo-operation-name");

        Assert.Equal("foo-operation-name", LastSubmittedEventTelemetry.Properties["OperationName"]);
    }
}
