using System.Collections.Generic;
using System.IO;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.ApplicationInsights.Formatters;

/// <summary>
/// Formats properties containing structured data as JSON.
/// </summary>
public class ApplicationInsightsJsonValueFormatter : IValueFormatter
{
    readonly JsonValueFormatter _formatter = new("$type");

    /// <inheritdoc />
    public void Format(
        string propertyName,
        LogEventPropertyValue propertyValue,
        IDictionary<string, string> properties)
    {
        string formattedValue;

        if (propertyValue is ScalarValue { Value: string literal })
        {
            formattedValue = literal;
        }
        else
        {
            using var sw = new StringWriter();
            _formatter.Format(propertyValue, sw);
            formattedValue = sw.ToString();
        }

        if (properties.ContainsKey(propertyName))
        {
            SelfLog.WriteLine("The key {0} is not unique after simplification. Ignoring new value {1}",
                propertyName, formattedValue);
            return;
        }

        properties.Add(propertyName, formattedValue);
    }
}
