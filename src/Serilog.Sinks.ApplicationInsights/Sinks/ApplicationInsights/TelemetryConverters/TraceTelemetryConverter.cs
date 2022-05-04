using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Serilog.Sinks.ApplicationInsights.TelemetryConverters
{
    public class TraceTelemetryConverter : TelemetryConverterBase
    {
        private static readonly MessageTemplateTextFormatter MessageTemplateTextFormatter = new MessageTemplateTextFormatter("{Message:l}");

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
}
