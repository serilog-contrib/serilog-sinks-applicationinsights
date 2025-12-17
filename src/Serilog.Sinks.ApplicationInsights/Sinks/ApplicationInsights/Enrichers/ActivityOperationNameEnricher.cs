// SPDX-FileCopyrightText: 2025 Serilog Contributors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Serilog.Sinks.ApplicationInsights.Enrichers;

/// <summary>
/// Enriches log events with the current operation name from <see cref="Activity"/>.
/// </summary>
public class ActivityOperationNameEnricher : ILogEventEnricher
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

        string operationName = Activity.Current?.OperationName;
        if (operationName is null)
        {
            return;
        }

        LogEventProperty operationNameProperty = propertyFactory.CreateProperty(TelemetryConverterBase.OperationNameProperty, operationName);
        logEvent.AddPropertyIfAbsent(operationNameProperty);
    }
}
