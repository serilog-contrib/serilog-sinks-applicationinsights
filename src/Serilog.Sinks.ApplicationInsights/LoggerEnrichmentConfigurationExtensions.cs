// SPDX-FileCopyrightText: 2025 Serilog Contributors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Serilog.Configuration;
using Serilog.Sinks.ApplicationInsights.Enrichers;

namespace Serilog;

/// <summary>
///     Provides enrichment extensions to <see cref="LoggerConfiguration" />.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    extension(LoggerEnrichmentConfiguration loggerEnrichmentConfiguration)
    {
        /// <summary>
        ///     Enriches log events with the operation name from <see cref="Activity" />.
        /// </summary>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public LoggerConfiguration WithOperationName()
            => loggerEnrichmentConfiguration.With<ActivityOperationNameEnricher>();

        /// <summary>
        ///     Enriches log events with the baggage from <see cref="Activity" />.
        /// </summary>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public LoggerConfiguration WithBaggage()
            => loggerEnrichmentConfiguration.With<ActivityBaggageEnricher>();
    }
}
