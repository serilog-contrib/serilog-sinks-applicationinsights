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
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Writes log events to a Microsoft Azure Application Insights account.
    /// </summary>
    public class ApplicationInsightsSink : ApplicationInsightsSinkBase
    {
        /// <summary>
        /// Creates a sink that saves logs as <see cref="ITelemetry"/> to the Application Insights account for the given <paramref name="telemetryClient" /> instance.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights <paramref name="telemetryClient" />.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <param name="logEventToTelemetryConverter">The <see cref="LogEvent" /> to <see cref="ITelemetry" /> converter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logEventToTelemetryConverter" /> is <see langword="null" />.</exception>
        public ApplicationInsightsSink(
            TelemetryClient telemetryClient,
            Func<LogEvent, IFormatProvider, ITelemetry> logEventToTelemetryConverter,
            IFormatProvider formatProvider = null)
            : base(telemetryClient, (e, f, c) => new[] { logEventToTelemetryConverter(e, f) }, formatProvider)
        {
            if (telemetryClient == null) throw new ArgumentNullException(nameof(telemetryClient));
            if (logEventToTelemetryConverter == null) throw new ArgumentNullException(nameof(logEventToTelemetryConverter));
        }

        /// <summary>
        /// Creates a sink that saves logs as <see cref="ITelemetry"/> to the Application Insights account for the given <paramref name="telemetryClient" /> instance.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights <paramref name="telemetryClient" />.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <param name="logEventToTelemetryConverter">The <see cref="LogEvent" /> to <see cref="ITelemetry" /> converter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logEventToTelemetryConverter" /> is <see langword="null" />.</exception>
        public ApplicationInsightsSink(
            TelemetryClient telemetryClient,
            Func<LogEvent, IFormatProvider, TelemetryClient, IEnumerable<ITelemetry>> logEventToTelemetryConverter,
            IFormatProvider formatProvider = null)
            : base(telemetryClient, logEventToTelemetryConverter, formatProvider)
        {
            if (telemetryClient == null) throw new ArgumentNullException(nameof(telemetryClient));
            if (logEventToTelemetryConverter == null) throw new ArgumentNullException(nameof(logEventToTelemetryConverter));
        }
    }
}