// Copyright 2016 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.ExtensionMethods;

namespace Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Writes log events as Events to a Microsoft Azure Application Insights account.
    /// </summary>
    public class ApplicationInsightsEventsSink : ApplicationInsightsSinkBase
    {
        /// <summary>
        /// Creates a sink that saves logs as Events to the Application Insights account for the given <paramref name="telemetryClient" /> instance.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights <paramref name="telemetryClient" />.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <param name="logEventToTelemetryConverter">The <see cref="LogEvent" /> to <see cref="ITelemetry" /> converter.</param>
        /// <exception cref="System.ArgumentNullException">telemetryClient</exception>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient" /> is <see langword="null" />.</exception>
        public ApplicationInsightsEventsSink(
            TelemetryClient telemetryClient,
            IFormatProvider formatProvider = null,
            Func<LogEvent, IFormatProvider, TelemetryClient, IEnumerable<ITelemetry>> logEventToTelemetryConverter = null)
            : base(telemetryClient, logEventToTelemetryConverter ?? DefaultLogEventToEventTelemetryConverter, formatProvider)
        {
            if (telemetryClient == null) throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <summary>
        /// Creates a sink that saves logs as Events to the Application Insights account for the given <paramref name="telemetryClient" /> instance.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights <paramref name="telemetryClient" />.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <param name="logEventToTelemetryConverter">The <see cref="LogEvent" /> to <see cref="ITelemetry" /> converter.</param>
        /// <exception cref="System.ArgumentNullException">telemetryClient</exception>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient" /> is <see langword="null" />.</exception>
        public ApplicationInsightsEventsSink(
            TelemetryClient telemetryClient,
            IFormatProvider formatProvider = null,
            Func<LogEvent, IFormatProvider, ITelemetry> logEventToTelemetryConverter = null)
            : this(telemetryClient, formatProvider,
                  logEventToTelemetryConverter == null
                    ? (Func<LogEvent, IFormatProvider, TelemetryClient, IEnumerable<ITelemetry>>)null
                    : (e, f, c) => new[] { logEventToTelemetryConverter(e, f) })
        {
        }


        /// <summary>
        /// Emits the provided <paramref name="logEvent" /> to AI as an <see cref="EventTelemetry" />.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">logEvent</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logEvent" /> is <see langword="null" />.</exception>
        private static IEnumerable<ITelemetry> DefaultLogEventToEventTelemetryConverter(LogEvent logEvent, IFormatProvider formatProvider, TelemetryClient telemetryClient)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            if (logEvent.Exception == null)
            {
                yield return logEvent.ToDefaultEventTelemetry(formatProvider);
            }
            else
            {
                yield return logEvent.ToDefaultExceptionTelemetry(formatProvider);
            }
        }
    }
}