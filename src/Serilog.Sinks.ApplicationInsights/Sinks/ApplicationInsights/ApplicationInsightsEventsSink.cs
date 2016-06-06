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
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Writes log events as Events to a Microsoft Azure Application Insights account.
    /// </summary>
    public class ApplicationInsightsEventsSink : ApplicationInsightsSink
    {
        /// <summary>
        /// Creates a sink that saves logs as Events to the Application Insights account for the given <paramref name="telemetryClient"/> instance.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights <paramref name="telemetryClient"/>.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient"/> is <see langword="null" />.</exception>
        public ApplicationInsightsEventsSink(TelemetryClient telemetryClient, IFormatProvider formatProvider = null)
            : base(telemetryClient, formatProvider)
        {
            if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
        }

        /// <summary>
        /// Emits the provided <paramref name="logEvent"/> to AI as an <see cref="EventTelemetry"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="logEvent"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="logEvent"/> must have a <see cref="LogEvent.Exception"/>.</exception>
        private void TrackAsEvent(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            CheckForAndThrowIfDisposed();

            var renderedMessage = logEvent.RenderMessage(FormatProvider);

            var eventTelemetry = new EventTelemetry(logEvent.MessageTemplate.Text)
            {
                Timestamp = logEvent.Timestamp
            };

            // write logEvent's .Properties to the AI one
            ForwardLogEventPropertiesToTelemetryProperties(eventTelemetry, logEvent, renderedMessage);

            TelemetryClient.TrackEvent(eventTelemetry);
        }

        #region Overrides of ApplicationInsightsSink

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public override void Emit(LogEvent logEvent)
        {
            CheckForAndThrowIfDisposed();

            if (logEvent.Exception != null)
            {
                TrackAsException(logEvent);
            }
            else
            {
                TrackAsEvent(logEvent);
            }
        }

        #endregion
    }
}