using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.TelemetryConverters
{
    public interface ITelemetryConverter
    {
        IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider);
    }
}
