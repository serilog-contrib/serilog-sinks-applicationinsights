using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.TelemetryConverters;

#pragma warning disable CS1591

public class EventTelemetryConverter : TelemetryConverterBase
{
    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

        if (logEvent.Exception == null)
        {
            var telemetry = new EventTelemetry(logEvent.MessageTemplate.Text) {
                Timestamp = logEvent.Timestamp
            };

            // write logEvent's .Properties to the AI one
            ForwardPropertiesToTelemetryProperties(logEvent, telemetry, formatProvider);

            yield return telemetry;
        }
        else
        {
            yield return ToExceptionTelemetry(logEvent, formatProvider);
        }
    }

    public override void ForwardPropertiesToTelemetryProperties(LogEvent logEvent,
        ISupportProperties telemetryProperties, IFormatProvider formatProvider)
    {
        ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
            includeLogLevel: false,
            includeRenderedMessage: true,
            includeMessageTemplate: false);
    }
}