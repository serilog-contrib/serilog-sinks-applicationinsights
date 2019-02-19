using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters
{
    public interface ITelemetryConverter
    {
        IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider);
    }
}
