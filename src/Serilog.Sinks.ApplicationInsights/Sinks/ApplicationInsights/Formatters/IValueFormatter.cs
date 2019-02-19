using Serilog.Events;
using System.Collections.Generic;

namespace Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.Formatters
{
    public interface IValueFormatter
    {
        void Format(string propertyName, LogEventPropertyValue propertyValue, IDictionary<string, string> properties);
    }
}
