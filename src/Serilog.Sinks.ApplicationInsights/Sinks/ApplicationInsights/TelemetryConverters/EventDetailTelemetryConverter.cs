using System;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters
{
    /// <summary>
    /// Similar to Events, however it allows logging LogLevel and Template
    /// </summary>
    public class EventDetailTelemetryConverter : EventTelemetryConverter
    {
        public override void ForwardPropertiesToTelemetryProperties(LogEvent logEvent, ISupportProperties telemetryProperties, IFormatProvider formatProvider)
        {
            ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
                includeLogLevel: true,
                includeRenderedMessage: true,
                includeMessageTemplate: true);
        }
    }
}
