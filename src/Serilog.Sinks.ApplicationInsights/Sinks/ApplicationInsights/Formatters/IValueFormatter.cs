using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.Formatters
{
    public interface IValueFormatter
    {
        void Format(string propertyName, LogEventPropertyValue propertyValue, IDictionary<string, string> properties);
    }
}
