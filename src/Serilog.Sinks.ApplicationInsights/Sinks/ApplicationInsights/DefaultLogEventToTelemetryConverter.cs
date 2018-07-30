using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.ExtensionMethods;
using System;
using Microsoft.ApplicationInsights.DataContracts;

namespace Serilog.Sinks.ApplicationInsights
{
    public class DefaultLogEventToTelemetryConverter<T> : ILogEventToTelemetryConverter
        where T : ITelemetry
    {
        /// <summary>
        /// Uses <see cref="LogEvent"/> to create a specific telemetry
        /// </summary>
        /// <param name="logEvent">current log event from sink</param>
        /// <param name="formatProvider">format provider for correct localization</param>
        /// <returns></returns>
        public ITelemetry Invoke(LogEvent logEvent, IFormatProvider formatProvider)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (logEvent.Exception == null)
            {
                return logEvent.ToDefaultTelemetry<T>(formatProvider);
            }

            return logEvent.ToDefaultTelemetry<ExceptionTelemetry>(formatProvider);
        }
    }
}
