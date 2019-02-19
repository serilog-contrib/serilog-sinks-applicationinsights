﻿using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters
{
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
    }
}
