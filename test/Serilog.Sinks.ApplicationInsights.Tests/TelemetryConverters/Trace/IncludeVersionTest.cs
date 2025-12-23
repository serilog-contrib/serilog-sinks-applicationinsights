using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Trace;

public class IncludeVersionTest : ApplicationInsightsTest
{
    public IncludeVersionTest()
        : base(new TraceTelemetryConverter(false, false, false, true, true), true, true)
    {
    }

    [Theory]
    [InlineData("Hello, {version}!", "version", "v1.3.3.7")]
    [InlineData("Hello, {Version}!", "Version", "v4.2.0")]
    public void VersionSetAsTraceProperty(string pattern, string versionKey, string expectedVersion)
    {
        using var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        Logger.Information(pattern, expectedVersion);

        Assert.Equal(expectedVersion, LastSubmittedTraceTelemetry.Properties[versionKey]);
    }
}
