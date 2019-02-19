using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.ExtensionMethods;

namespace Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters
{
    public class TraceTelemetryConverter : ITelemetryConverter
    {
        public TraceTelemetryConverter()
        {

        }


        public IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            string renderedMessage = logEvent.RenderMessage(formatProvider);

            var telemetry = new TraceTelemetry(renderedMessage)
            {
                Timestamp = logEvent.Timestamp,
                SeverityLevel = logEvent.Level.ToSeverityLevel()
            };

            logEvent.ForwardPropertiesToTelemetryProperties(telemetry, formatProvider, false, false, false, false);

            yield return telemetry;
        }
    }
}
