using System.Diagnostics;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.TelemetryConverters.Event;

public class EventTelemetryConverterTest : ApplicationInsightsTest
{
    public EventTelemetryConverterTest() : base(new EventTelemetryConverter(), true, true)
    {
    }

    [Fact]
    public void MessagesAreFormattedWithoutQuotedStrings()
    {
        Logger.Information("Hello, {Name}!", "world");
        Assert.Equal("Hello, world!", LastSubmittedEventTelemetry.Properties["RenderedMessage"]);
    }

    [Fact]
    public void MessagesAreFormattedWithoutQuotedStringsWhenDestructuring()
    {
        Logger.Information("Hello, {@Name}", new { Foo = "foo", Bar = 123 });
        Assert.Equal("Hello, {\"Foo\":\"foo\",\"Bar\":123}", LastSubmittedEventTelemetry.Properties["RenderedMessage"]);
    }

    [Fact]
    public void MessageQuotesAreNotEscaped()
    {
        Logger.Information("Data: {MyData}", "This string is \"quoted\"");
        Assert.Equal("Data: This string is \"quoted\"", LastSubmittedEventTelemetry.Properties["RenderedMessage"]);
    }

    [Fact]
    public void DestructuredPropertyIsFormattedCorrectly()
    {
        Logger.Information("Hello, {@MyData}", new { Foo = "foo", Bar = 123 });
        Assert.Equal("{\"Foo\":\"foo\",\"Bar\":123}", LastSubmittedEventTelemetry.Properties["MyData"]);
    }

    [Fact]
    public void TraceIdIsNullByDefault()
    {
        Logger.Information("Hello, {Name}!", "world");
        Assert.Null(LastSubmittedEventTelemetry.Context.Operation.Id);
    }

    [Fact]
    public void TraceIdIsSet()
    {
        using Activity activity = new("TestActivity");
        activity.Start();
        Logger.Information("Hello, {Name}!", "world");
        Assert.Equal(activity.TraceId.ToHexString(), LastSubmittedEventTelemetry.Context.Operation.Id);
    }

    [Theory]
    [InlineData("Hello, {operationId}!")]
    [InlineData("Hello, {OperationId}!")]
    public void OperationIdTakesPrecedenceOverTraceId(string messageTemplate)
    {
        using Activity activity = new("TestActivity");
        activity.Start();
        string operationId = Guid.NewGuid().ToString("N");
        Logger.Information(messageTemplate, operationId);
        Assert.Equal(operationId, LastSubmittedEventTelemetry.Context.Operation.Id);
        Assert.Null(LastSubmittedEventTelemetry.Context.Operation.ParentId);
    }

    [Fact]
    public void ParentSpanIdIsSet()
    {
        Logger.Information("Test {ParentSpanId}", "parent123");
        Assert.Equal("parent123", LastSubmittedEventTelemetry.Context.Operation.ParentId);
    }

    [Fact]
    public void VersionIsSet()
    {
        Logger.Information("Test {version}", "1.2.3");
        Assert.Equal("1.2.3", LastSubmittedEventTelemetry.Context.Component.Version);
    }

    [Fact]
    public void OperationNameIsSet()
    {
        using Activity activity = new("MyOperation");
        activity.Start();

        Logger.Information("Test");

        Assert.Equal("MyOperation", LastSubmittedEventTelemetry.Context.Operation.Name);
    }

    [Fact]
    public void BaggageIsSet()
    {
        using Activity activity = new("TestActivity");
        activity.AddBaggage("key1", "value1");
        activity.AddBaggage("key2", "value2");
        activity.Start();

        Logger.Information("Hello, world!");

        Assert.Equal("value1", LastSubmittedEventTelemetry.Properties["key1"]);
        Assert.Equal("value2", LastSubmittedEventTelemetry.Properties["key2"]);
    }
}
