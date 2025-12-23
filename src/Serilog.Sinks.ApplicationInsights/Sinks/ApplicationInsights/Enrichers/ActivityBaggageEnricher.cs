// SPDX-FileCopyrightText: 2025 Serilog Contributors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Serilog.Sinks.ApplicationInsights.Enrichers;

/// <summary>
/// Enriches log events with the baggage from <see cref="Activity"/>.
/// </summary>
public class ActivityBaggageEnricher : ILogEventEnricher
{
    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent == null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        if (propertyFactory == null)
        {
            throw new ArgumentNullException(nameof(propertyFactory));
        }

        if (Activity.Current is not {} activity)
        {
            return;
        }

        IEnumerable<LogEventProperty> items = activity.Baggage
            .Where(IsValidBaggageItem)
            .Select(item => propertyFactory.CreateProperty(item.Key, item.Value));

        LogEventProperty baggageProperty = new(TelemetryConverterBase.BaggageProperty, new StructureValue(items));
        logEvent.AddPropertyIfAbsent(baggageProperty);
    }

    private static bool IsValidBaggageItem(KeyValuePair<string, string> item)
        => !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value);
}
