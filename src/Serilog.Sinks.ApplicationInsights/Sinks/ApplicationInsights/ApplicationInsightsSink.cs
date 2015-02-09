﻿// Copyright 2013 Serilog Contributors
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
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
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
        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Holds the actual Application Insights TelemetryClient that will be used for logging.
        /// </summary>
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Construct a sink that saves logs to the specified storage account.
        /// </summary>
        /// <param name="applicationInsightsInstrumentationKey">The ID that determines the application component under which your data appears in Application Insights.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        public ApplicationInsightsSink(string applicationInsightsInstrumentationKey = null,
            IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrWhiteSpace(applicationInsightsInstrumentationKey) == false)
                Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = applicationInsightsInstrumentationKey;

            _telemetryClient = new TelemetryClient(Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active);
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

            // writing logEvent as corresponding ITelemetry instance
            var telemetry = logEvent.Exception != null
                ? (ITelemetry)new ExceptionTelemetry(logEvent.Exception)
                : new TraceTelemetry(renderedMessage);

            // and forwaring properties and logEvent Data to the traceTelemetry's properties
            var properties = telemetry.Context.Properties;
            properties.Add("LogLevel", logEvent.Level.ToString());
            properties.Add("LogMessage", renderedMessage);
            properties.Add("LogTimeStamp", logEvent.Timestamp.ToString(CultureInfo.InvariantCulture));

            foreach (var property in logEvent.Properties.Where(property => property.Value != null && !properties.ContainsKey(property.Key)))
            {
                switch (property.Key.ToLower(CultureInfo.InvariantCulture))
                {
                    case "username":
                        telemetry.Context.User.AccountId = property.Value.ToString();
                        break;
                    case "httprequestuseragent":
                        telemetry.Context.User.UserAgent = property.Value.ToString();
                        break;
                    case "httpsessionid":
                        telemetry.Context.Session.Id = property.Value.ToString();
                        break;
                    case "httprequestclienthostip":
                        telemetry.Context.Location.Ip = property.Value.ToString();
                        break;
                    default:
                        properties.Add(property.Key, property.Value.ToString());
                        break;
                }
            }

            // an finally - this logs the message & its metadata to application insights
            _telemetryClient.Track(telemetry);
        }

        #endregion
    }
}