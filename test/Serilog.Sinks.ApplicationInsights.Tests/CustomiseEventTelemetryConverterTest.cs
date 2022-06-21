using System;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Serilog.Sinks.ApplicationInsights.Tests;

public class CustomiseEventTelemetryConverterTest : ApplicationInsightsTest
{
    class IncludeRenderedMessageConverter : EventTelemetryConverter
    {
        public override void ForwardPropertiesToTelemetryProperties(LogEvent logEvent,
            ISupportProperties telemetryProperties, IFormatProvider formatProvider)
        {
            base.ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
                includeLogLevel: false,
                includeRenderedMessage: true,
                includeMessageTemplate: false);
        }
    }
}