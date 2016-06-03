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
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Base class for Microsoft Azure Application Insights based Sinks.
    /// Inspired by their NLog Appender implementation.
    /// </summary>
    public abstract class ApplicationInsightsSink : ILogEventSink, IDisposable
    {
        /// <summary>
        /// The format provider
        /// </summary>
        protected IFormatProvider FormatProvider { get; private set; }

        /// <summary>
        /// Holds the actual Application Insights TelemetryClient that will be used for logging.
        /// </summary>
        protected TelemetryClient TelemetryClient { get; private set; }

        /// <summary>
        /// Creates a sink that saves logs to the Application Insights account for the given <paramref name="telemetryClient"/> instance.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights <paramref name="telemetryClient"/>.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient"/> cannot be null</exception>
        protected ApplicationInsightsSink(TelemetryClient telemetryClient, IFormatProvider formatProvider = null)
        {
            if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");

            TelemetryClient = telemetryClient;
            FormatProvider = formatProvider;
        }

        #region AI specifc Helper methods

        /// <summary>
        /// Emits the provided <paramref name="logEvent"/> to AI as an <see cref="ExceptionTelemetry"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="logEvent"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="logEvent"/> must have a <see cref="LogEvent.Exception"/>.</exception>
        protected void TrackAsException(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");
            if (logEvent.Exception == null) throw new ArgumentException("Must have an Exception", "logEvent");

            var renderedMessage = logEvent.RenderMessage(FormatProvider);
            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception)
            {
                SeverityLevel = logEvent.Level.ToSeverityLevel(),
                HandledAt = ExceptionHandledAt.UserCode,
                Timestamp = logEvent.Timestamp
            };

            // write logEvent's .Properties to the AI one
            ForwardLogEventPropertiesToTelemetryProperties(exceptionTelemetry, logEvent, renderedMessage);

            TelemetryClient.TrackException(exceptionTelemetry);
        }

        /// <summary>
        /// Forwards the log event properties to the provided <see cref="ISupportProperties" /> instance.
        /// </summary>
        /// <param name="telemetry">The telemetry.</param>
        /// <param name="logEvent">The log event.</param>
        /// <param name="renderedMessage">The rendered message.</param>
        /// <returns></returns>
        protected void ForwardLogEventPropertiesToTelemetryProperties(ISupportProperties telemetry, LogEvent logEvent, string renderedMessage)
        {
            telemetry.Properties.Add("LogLevel", logEvent.Level.ToString());
            telemetry.Properties.Add("RenderedMessage", renderedMessage);

            foreach (var property in logEvent.Properties.Where(property => property.Value != null && !telemetry.Properties.ContainsKey(property.Key)))
            {
                telemetry.Properties.Add(property.Key, property.Value.ToString());
            }
        }

        #endregion AI specifc Helper methods

        #region Implementation of ILogEventSink

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public abstract void Emit(LogEvent logEvent);

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Flush the app insights buffer when disposed
        /// e.g Serilog 2 - Log.CloseAndFlush();
        /// </summary>
        public void Dispose()
        {
            TelemetryClient.Flush();
            System.Threading.Thread.Sleep(1000);
        }

        #endregion
    }
}
