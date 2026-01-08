using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Trace;

public class IncludeOperationIdTest : ApplicationInsightsTest
{
    public IncludeOperationIdTest()
        : base(new TraceTelemetryConverter(true, false, false, false, true), true, true)
    {
    }

    [Theory]
    [InlineData("Hello, {operationId}!", "operationId", "foo-operation-id")]
    [InlineData("Hello, {OperationId}!", "OperationId", "bar-operation-id")]
    public void OperationIdIsSetAsTraceProperty(string pattern, string operationIdKey, string expectedOperationId)
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information(pattern, expectedOperationId);

        Assert.Equal(expectedOperationId, LastSubmittedTraceTelemetry.Properties[operationIdKey]);
    }
}
