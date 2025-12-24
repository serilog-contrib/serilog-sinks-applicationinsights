using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Serilog.Sinks.ApplicationInsights.TelemetryConverters;

#pragma warning disable CS1591

public class TraceTelemetryConverter : TelemetryConverterBase
{
    static readonly MessageTemplateTextFormatter MessageTemplateTextFormatter = new("{Message:lj}");

    /// <inheritdoc cref="EventTelemetryConverter(bool, bool, bool, bool, bool)"/>
    public TraceTelemetryConverter()
        : this(false, false, false, false, true)
    {
    }

    /// <inheritdoc cref="TelemetryConverterBase(bool, bool, bool, bool, bool)"/>
    public TraceTelemetryConverter(
        bool includeOperationIdPropertyAsTelemetryProperty,
        bool includeParentSpanIdPropertyAsTelemetryProperty,
        bool includeOperationNamePropertyAsTelemetryProperty,
        bool includeVersionPropertyAsTelemetryProperty,
        bool ignorePropertyNameCase)
        : base(
            includeOperationIdPropertyAsTelemetryProperty,
            includeParentSpanIdPropertyAsTelemetryProperty,
            includeOperationNamePropertyAsTelemetryProperty,
            includeVersionPropertyAsTelemetryProperty,
            ignorePropertyNameCase)
    {
    }

    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        if (logEvent == null)
            throw new ArgumentNullException(nameof(logEvent));

        if (logEvent.Exception == null)
        {
            var sw = new StringWriter();
            MessageTemplateTextFormatter.Format(logEvent, sw);

            var telemetry = new TraceTelemetry(sw.ToString())
            {
                Timestamp = logEvent.Timestamp,
                SeverityLevel = ToSeverityLevel(logEvent.Level)
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
}
