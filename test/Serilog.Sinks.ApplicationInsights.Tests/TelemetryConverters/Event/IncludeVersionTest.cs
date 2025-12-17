using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Event;

public class IncludeVersionTest : ApplicationInsightsTest
{
    public IncludeVersionTest()
        : base(new EventTelemetryConverter(false, false, false, true), true, true)
    {
    }

    [Fact]
    public void OperationIdIsSetAsTraceProperty()
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information("Hello, {version}!", "v1.3.3.7");

        Assert.Equal("v1.3.3.7", LastSubmittedEventTelemetry.Properties["version"]);
    }
}
