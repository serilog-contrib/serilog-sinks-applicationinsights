using Serilog.Sinks.ApplicationInsights.Formatters;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests;

public class DottedOutFormattingTest : ApplicationInsightsTest
{
    public DottedOutFormattingTest() : base(new DottedOutTrace())
    {
    }

    [Fact]
    public void Json_parameter_is_dotted_out()
    {
        var position = new { Latitude = 25, Longitude = 134 };
        var elapsedMs = 34;
        var numbers = new[] { 1, 2, 3, 4 };

        Logger.Information("Processed {@Position} in {Elapsed:000} ms., str {str}, numbers: {numbers}", position,
            elapsedMs, "test", numbers);

        Assert.Equal("34", LastSubmittedTraceTelemetry.Properties["Elapsed"]);
        Assert.Equal("25", LastSubmittedTraceTelemetry.Properties["Position.Latitude"]);
        Assert.Equal("134", LastSubmittedTraceTelemetry.Properties["Position.Longitude"]);
        Assert.Equal("1", LastSubmittedTraceTelemetry.Properties["numbers.0"]);
        Assert.Equal("2", LastSubmittedTraceTelemetry.Properties["numbers.1"]);
        Assert.Equal("3", LastSubmittedTraceTelemetry.Properties["numbers.2"]);
        Assert.Equal("4", LastSubmittedTraceTelemetry.Properties["numbers.3"]);
    }

    class DottedOutTrace : TraceTelemetryConverter
    {
        public override IValueFormatter ValueFormatter => new ApplicationInsightsDottedValueFormatter();
    }
}