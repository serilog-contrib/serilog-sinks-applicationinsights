using System.Collections.Generic;
using System.IO;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.ApplicationInsights.Formatters;

#pragma warning disable CS1591

public class ApplicationInsightsJsonValueFormatter : IValueFormatter
{
    static readonly char[] TrimChars = { '\"' };
    readonly JsonValueFormatter _formatter = new();

    public void Format(string propertyName, LogEventPropertyValue propertyValue,
        IDictionary<string, string> properties)
    {
        string value;
        using (var sw = new StringWriter())
        {
            _formatter.Format(propertyValue, sw);
            value = sw.ToString();
        }

        value = value.Trim(TrimChars);

        if (properties.ContainsKey(propertyName))
        {
            SelfLog.WriteLine("The key {0} is not unique after simplification. Ignoring new value {1}",
                propertyName, value);
            return;
        }

        properties.Add(propertyName, value);
    }
}