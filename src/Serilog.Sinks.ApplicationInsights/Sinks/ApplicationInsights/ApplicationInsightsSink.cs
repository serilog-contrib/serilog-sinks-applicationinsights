// Copyright 2013 Serilog Contributors
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
using System.Globalization;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Writes log events to a Microsoft Azure Application Insights account.
    /// Inspired by their NLog Appender implementation.
    /// </summary>
    public class ApplicationInsightsSink : ILogEventSink
    {
        /// <summary>
        /// The format provider
        /// </summary>
        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Holds the actual Application Insights TelemetryClient that will be used for logging.
        /// </summary>
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Construct a sink that saves logs to the specified storage account.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights telemetryClient.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <exception cref="System.ArgumentNullException">telemetryClient</exception>
        public ApplicationInsightsSink(TelemetryClient telemetryClient, IFormatProvider formatProvider = null)
        {
            if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");

            _telemetryClient = telemetryClient;
            _formatProvider = formatProvider;
        }
        
        #region Implementation of ILogEventSink

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent)
        {
            var renderedMessage = logEvent.RenderMessage(_formatProvider);

            // take logEvent and use it for the corresponding ITelemetry counterpart
            if (logEvent.Exception != null)
            {
                var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception)
                {
                    SeverityLevel = logEvent.Level.ToSeverityLevel()
                };
                
                // write logEvent's .Properties to the AI one
                ForwardLogEventPropertiesToTelemetryProperties(exceptionTelemetry, logEvent, renderedMessage);

                _telemetryClient.Track(exceptionTelemetry);
            }
            else
            {
                var eventTelemetry = new EventTelemetry(renderedMessage);

                // write logEvent's .Properties to the AI one
                ForwardLogEventPropertiesToTelemetryProperties(eventTelemetry, logEvent, renderedMessage);
                
                _telemetryClient.Track(eventTelemetry);
            }
        }

        /// <summary>
        /// Forwards the log event properties to the provided <see cref="ISupportProperties" /> instance.
        /// </summary>
        /// <param name="telemetry">The telemetry.</param>
        /// <param name="logEvent">The log event.</param>
        /// <param name="renderedMessage">The rendered message.</param>
        /// <returns></returns>
        private void ForwardLogEventPropertiesToTelemetryProperties(ISupportProperties telemetry, LogEvent logEvent, string renderedMessage)
        {
            telemetry.Properties.Add("LogLevel", logEvent.Level.ToString());
            telemetry.Properties.Add("LogMessage", renderedMessage);
            telemetry.Properties.Add("LogTimeStamp", logEvent.Timestamp.ToString(CultureInfo.InvariantCulture));
            
            foreach (var property in logEvent.Properties.Where(property => property.Value != null && !telemetry.Properties.ContainsKey(property.Key)))
            {
                telemetry.Properties.Add(property.Key, property.Value.ToString());
            }
        }

        #endregion
    }
}
