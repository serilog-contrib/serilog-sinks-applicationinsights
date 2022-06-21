using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.Formatters;

/// <summary>
/// Convert Serilog log event properties into flat <code>(string, string)</code> pairs to send to Application Insights.
/// </summary>
public interface IValueFormatter
{
    /// <summary>
    /// Convert the log event property <paramref name="propertyName"/> with value <paramref name="propertyValue"/>
    /// into one or more key-value properties to send to Application Insights, adding these to <paramref name="properties"/>.
    /// </summary>
    /// <param name="propertyName">The Serilog log event's name for the property.</param>
    /// <param name="propertyValue">The log event's property value.</param>
    /// <param name="properties">The collection of string properties being built to send to Application Insights.</param>
    void Format(string propertyName, LogEventPropertyValue propertyValue, IDictionary<string, string> properties);
}
