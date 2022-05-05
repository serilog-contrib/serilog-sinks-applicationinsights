using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.ApplicationInsights.Formatters;

#nullable enable

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
        if (propertyValue is ScalarValue sv)
        {
            formattedValue = sv.Value switch
            {
                // In logs, being able to distinguish null from an empty string is often important.
                null => "null",
                // ISO-8601 is most accurate to parse.
                DateTime or DateTimeOffset => ((IFormattable)sv.Value).ToString("o", CultureInfo.InvariantCulture),
                char c => c.ToString(),
                // Serilog's JSON representation of these values is unquoted and generally more suitable for
                // parsing/processing than their default culture-dependent `ToString()` representations.
                int or uint or long or ulong or decimal or byte or sbyte or short or ushort or double or 
                    float or bool => FormatAsJson(sv),
                _ => sv.Value.ToString() ?? "(null)"
            };
        }
        else
        {
            formattedValue = FormatAsJson(propertyValue);
        }

        if (properties.ContainsKey(propertyName))
        {
            SelfLog.WriteLine("The key {0} is not unique after simplification. Ignoring new value {1}",
                propertyName, formattedValue);
            return;
        }

        properties.Add(propertyName, formattedValue);
    }

    string FormatAsJson(LogEventPropertyValue propertyValue)
    {
        using var sw = new StringWriter();
        _formatter.Format(propertyValue, sw);
        return sw.ToString();
    }
}
