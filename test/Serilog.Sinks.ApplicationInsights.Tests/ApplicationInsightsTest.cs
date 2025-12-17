using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Serilog.Sinks.ApplicationInsights.Tests;

public abstract class ApplicationInsightsTest
{
    readonly UnitTestTelemetryChannel _channel;

    protected ApplicationInsightsTest(ITelemetryConverter converter = null, bool addOperationNameEnricher = false, bool addBaggageEnricher = false)
    {
        var tc = new TelemetryConfiguration { TelemetryChannel = _channel = new UnitTestTelemetryChannel() };

        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.ApplicationInsights(tc, converter ?? TelemetryConverter.Traces)
            .MinimumLevel.Debug()
            .Enrich.FromLogContext();

        if (addOperationNameEnricher)
        {
            loggerConfiguration = loggerConfiguration.Enrich.WithOperationName();
        }

        if (addBaggageEnricher)
        {
            loggerConfiguration = loggerConfiguration.Enrich.WithBaggage();
        }

        Logger = loggerConfiguration.CreateLogger();
    }

    protected ILogger Logger { get; }
    protected UnitTestTelemetryChannel Channel => _channel;

    protected List<ITelemetry> SubmittedTelemetry => _channel.SubmittedTelemetry;

    protected ITelemetry LastSubmittedTelemetry => _channel.SubmittedTelemetry.LastOrDefault();

    protected TraceTelemetry LastSubmittedTraceTelemetry =>
        _channel.SubmittedTelemetry
            .OfType<TraceTelemetry>()
            .LastOrDefault();

    protected EventTelemetry LastSubmittedEventTelemetry =>
        _channel.SubmittedTelemetry
            .OfType<EventTelemetry>()
            .LastOrDefault();
}
