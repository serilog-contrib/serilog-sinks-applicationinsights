using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Trace;

public class IncludeOperationNameTest : ApplicationInsightsTest
{
    public IncludeOperationNameTest()
        : base(new TraceTelemetryConverter(false, false, true, false, true), true, true)
    {
    }

    [Theory]
    [InlineData("Hello, {operationName}!", "operationName", "foo-operation-name")]
    [InlineData("Hello, {OperationName}!", "OperationName", "bar-operation-name")]
    public void OperationIdIsSetAsTraceProperty(string pattern, string operationNameKey, string expectedOperationName)
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information(pattern, expectedOperationName);

        Assert.Equal(expectedOperationName, LastSubmittedTraceTelemetry.Properties[operationNameKey]);
    }
}
