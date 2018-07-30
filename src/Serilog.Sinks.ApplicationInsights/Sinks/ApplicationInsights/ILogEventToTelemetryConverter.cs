using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using System;

namespace Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Definition of telemtry converter
    /// </summary>
    public interface ILogEventToTelemetryConverter
    {
        /// <summary>
        /// Uses <see cref="LogEvent"/> to create a specific telemetry
        /// </summary>
        /// <param name="logEvent">current log event from sink</param>
        /// <param name="formatProvider">format provider for correct localization</param>
        /// <returns></returns>
        ITelemetry Invoke(LogEvent logEvent, IFormatProvider formatProvider);
    }
}
