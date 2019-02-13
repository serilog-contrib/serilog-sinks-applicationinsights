using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.Formatters
{
    interface IValueFormatter
    {
        void Format(string propertyName, LogEventPropertyValue propertyValue, IDictionary<string, string> properties);
    }
}
