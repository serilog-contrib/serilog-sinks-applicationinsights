using System;
using System.Runtime.InteropServices;
using Serilog.Context;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests;

public class FormattingTests : ApplicationInsightsTest
{
    [Fact]
    public void Log_level_is_not_in_trace_custom_property()
    {
        Logger.Information("test");

        Assert.False(LastSubmittedTraceTelemetry.Properties.ContainsKey("LogLevel"));
    }

    [Fact]
    public void Message_template_is_in_trace_custom_property()
    {
        Logger.Information("test");

        Assert.True(LastSubmittedTraceTelemetry.Properties.ContainsKey("MessageTemplate"));
    }

    [Fact]
    public void Message_properties_include_log_context()
    {
        using (LogContext.PushProperty("custom1", "value1"))
        {
            Logger.Information("test context");

            Assert.True(LastSubmittedTraceTelemetry.Properties.TryGetValue("custom1", out var value1) &&
                        value1 == "value1");
        }
    }

    [Fact]
    public void Json_parameter_is_compact()
    {
        var position = new { Latitude = 25, Longitude = 134 };
        var elapsedMs = 34;
        var numbers = new[] { 1, 2, 3, 4 };

        Logger.Information("Processed {@Position} in {Elapsed:000} ms., str {str}, numbers: {numbers}", position,
            elapsedMs, "test", numbers);

        Assert.Equal("34", LastSubmittedTraceTelemetry.Properties["Elapsed"]);
        Assert.Equal("{\"Latitude\":25,\"Longitude\":134}", LastSubmittedTraceTelemetry.Properties["Position"]);
        Assert.Equal("[1,2,3,4]", LastSubmittedTraceTelemetry.Properties["numbers"]);
    }

    [Fact]
    public void OperationId_from_logContext_is_included()
    {
        using (LogContext.PushProperty("operationId", "myId1"))
        {
            Logger.Information("capture id?");

            Assert.Equal("myId1", LastSubmittedTraceTelemetry.Context.Operation.Id);
        }
    }

    [Fact]
    public void Version_from_logContext_is_included()
    {
        using (LogContext.PushProperty("version", "myId1"))
        {
            Logger.Information("capture id?");

            Assert.Equal("myId1", LastSubmittedTraceTelemetry.Context.Component.Version);
        }
    }

    [Theory]
    [InlineData("\"some \\ \"literal string \"", "\"some \\ \"literal string \"")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(null, "null")]
    [InlineData(123.45, "123.45")]
    [InlineData(6789, "6789")]
    [InlineData('a', "a")]
    public void Scalar_values_are_encoded_as_expected(object scalar, string expected)
    {
        Logger.Information("Value is {Scalar}", scalar);
        Assert.Equal(expected, LastSubmittedTraceTelemetry.Properties["Scalar"]);
    }
}
