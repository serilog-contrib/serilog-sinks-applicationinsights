﻿// ---------------------------------------------// Copyright 2016 Serilog Contributors
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
using System.IO;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.ApplicationInsights;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.Formatters;

namespace Serilog.ExtensionMethods
{
    /// <summary>
    /// Extension Methods for <see cref="LogEvent"/> instances.
    /// </summary>
    public static class LogEventExtensions
    {
        /// <summary>
        /// The <see cref="LogEvent.Level"/> is forwarded to the underlying AI Telemetry and its .Properties using this key.
        /// </summary>
        public const string TelemetryPropertiesLogLevel = "LogLevel";

        /// <summary>
        /// The <see cref="LogEvent.MessageTemplate"/> is forwarded to the underlying AI Telemetry and its .Properties using this key.
        /// </summary>
        public const string TelemetryPropertiesMessageTemplate = "MessageTemplate";

        /// <summary>
        /// The result of <see cref="LogEvent.RenderMessage(System.IFormatProvider)"/> is forwarded to the underlying AI Telemetry and its .Properties using this key.
        /// </summary>
        public const string TelemetryPropertiesRenderedMessage = "RenderedMessage";

        private static readonly IValueFormatter _defaultFormatter = new ApplicationInsightsDefaultValueFormatter();
        private static readonly IValueFormatter _jsonFormatter = new ApplicationInsightsJsonValueFormatter();



        /// <summary>
        /// Forwards all <see cref="LogEvent" /> data to the <paramref name="telemetryProperties" /> including the log level,
        /// rendered message, message template and all <paramref name="logEvent" /> properties to the telemetry.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="telemetryProperties">The telemetry properties.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="includeLogLevel">if set to <c>true</c> the <see cref="LogEvent.Level"/> is added to the
        /// <paramref name="telemetryProperties"/> using the <see cref="TelemetryPropertiesLogLevel"/> key.</param>
        /// <param name="includeRenderedMessage">if set to <c>true</c> the <see cref="LogEvent.RenderMessage(System.IFormatProvider)"/> output is added to the
        /// <paramref name="telemetryProperties"/> using the <see cref="TelemetryPropertiesRenderedMessage"/> key.</param>
        /// <param name="includeMessageTemplate">if set to <c>true</c> the <see cref="LogEvent.MessageTemplate"/> is added to the
        /// <paramref name="telemetryProperties"/> using the <see cref="TelemetryPropertiesMessageTemplate"/> key.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="logEvent" /> or <paramref name="telemetryProperties" /> is null.</exception>
        [Obsolete("this method will be removed in next versions, please use an overload with ApplicationInsightsOptions instead")]
        public static void ForwardPropertiesToTelemetryProperties(this LogEvent logEvent,
            ISupportProperties telemetryProperties,
            IFormatProvider formatProvider,
            bool includeLogLevel = true,
            bool includeRenderedMessage = true,
            bool includeMessageTemplate = true,
            bool useJsonFormatter = false)
        {
        }

        public static void ForwardPropertiesToTelemetryProperties(this LogEvent logEvent,
            ISupportProperties telemetryProperties,
            IFormatProvider formatProvider,
            ApplicationInsightsOptions options)
        { 
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (telemetryProperties == null) throw new ArgumentNullException(nameof(telemetryProperties));

            if (options.IncludeLogLevel)
            {
                telemetryProperties.Properties.Add(TelemetryPropertiesLogLevel, logEvent.Level.ToString());
            }

            if (options.IncludeRenderedMessage)
            {
                telemetryProperties.Properties.Add(TelemetryPropertiesRenderedMessage, logEvent.RenderMessage(formatProvider));
            }

            if (options.IncludeMessageTemplate)
            {
                telemetryProperties.Properties.Add(TelemetryPropertiesMessageTemplate, logEvent.MessageTemplate.Text);
            }

            IValueFormatter formatter = options.UseJsonFormatter ? _jsonFormatter : _defaultFormatter;

            foreach (KeyValuePair<string, LogEventPropertyValue> property in logEvent.Properties.Where(property => property.Value != null && !telemetryProperties.Properties.ContainsKey(property.Key)))
            {
                formatter.Format(property.Key, property.Value, telemetryProperties.Properties);
            }
        }

        /// <summary>
        /// Converts the provided <paramref name="logEvent" /> to an AI <see cref="ExceptionTelemetry" />.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="includeLogLevelAsProperty">if set to <c>true</c> the <see cref="LogEvent.Level"/> is added to the
        /// created <see cref="ExceptionTelemetry.Properties"/> using the <see cref="TelemetryPropertiesLogLevel"/> key.</param>
        /// <param name="includeRenderedMessageAsProperty">if set to <c>true</c> the <see cref="LogEvent.RenderMessage(System.IFormatProvider)"/> output is added to the
        /// created <see  cref="ExceptionTelemetry.Properties"/> using the <see cref="TelemetryPropertiesRenderedMessage"/> key.</param>
        /// <param name="includeMessageTemplateAsProperty">if set to <c>true</c> the <see cref="LogEvent.MessageTemplate"/> is added to the
        /// created <see cref="ExceptionTelemetry.Properties"/> using the <see cref="TelemetryPropertiesMessageTemplate"/> key.</param>
        /// <returns>An <see cref="ExceptionTelemetry"/> for the <paramref name="logEvent"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="logEvent" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="logEvent" /> must have a <see cref="LogEvent.Exception" />.</exception>
        public static ExceptionTelemetry ToDefaultExceptionTelemetry(
            this LogEvent logEvent,
            IFormatProvider formatProvider,
            bool includeLogLevelAsProperty = true,
            bool includeRenderedMessageAsProperty = true,
            bool includeMessageTemplateAsProperty = true,
            bool useJsonFormatter = false)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (logEvent.Exception == null) throw new ArgumentException("Must have an Exception", nameof(logEvent));

            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception)
            {
                SeverityLevel = logEvent.Level.ToSeverityLevel(),
                Timestamp = logEvent.Timestamp
            };

            // write logEvent's .Properties to the AI one
            logEvent.ForwardPropertiesToTelemetryProperties(
                exceptionTelemetry,
                formatProvider,
                includeLogLevelAsProperty,
                includeRenderedMessageAsProperty,
                includeMessageTemplateAsProperty,
                useJsonFormatter);

            return exceptionTelemetry;
        }

        /// <summary>
        /// Converts the provided <paramref name="logEvent" /> to an AI <see cref="EventTelemetry" />.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="includeLogLevelAsProperty">if set to <c>true</c> the <see cref="LogEvent.Level"/> is added to the
        /// created <see cref="ITelemetry"/> Properties using the <see cref="TelemetryPropertiesLogLevel"/> key.</param>
        /// <param name="includeRenderedMessageAsProperty">if set to <c>true</c> the <see cref="LogEvent.RenderMessage(System.IFormatProvider)"/> output is added to the
        /// created <see cref="ITelemetry"/> Properties using the <see cref="TelemetryPropertiesRenderedMessage"/> key.</param>
        /// <param name="includeMessageTemplateAsProperty">if set to <c>true</c> the <see cref="LogEvent.MessageTemplate"/> is added to the
        /// created <see cref="ITelemetry"/> Properties using the <see cref="TelemetryPropertiesMessageTemplate"/> key.</param>
        /// <returns>An <see cref="EventTelemetry"/> for the <paramref name="logEvent"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="logEvent" /> is <see langword="null" />.</exception>
        public static EventTelemetry ToDefaultEventTelemetry(
            this LogEvent logEvent,
            IFormatProvider formatProvider,
            bool includeLogLevelAsProperty = true,
            bool includeRenderedMessageAsProperty = true,
            bool includeMessageTemplateAsProperty = true,
            bool useJsonFormatter = false)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var telemetry = new EventTelemetry(logEvent.MessageTemplate.Text)
            {
                Timestamp = logEvent.Timestamp
            };

            // write logEvent's .Properties to the AI one
            logEvent.ForwardPropertiesToTelemetryProperties(telemetry,
                formatProvider,
                includeLogLevelAsProperty,
                includeRenderedMessageAsProperty,
                includeMessageTemplateAsProperty,
                useJsonFormatter);

            return telemetry;
        }

        /// <summary>
        /// Converts the provided <paramref name="logEvent" /> to an AI <see cref="TraceTelemetry" />.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="includeLogLevelAsProperty">if set to <c>true</c> the <see cref="LogEvent.Level"/> is added to the
        /// created <see cref="ITelemetry"/> Properties using the <see cref="TelemetryPropertiesLogLevel"/> key.</param>
        /// <param name="includeRenderedMessageAsProperty">if set to <c>true</c> the <see cref="LogEvent.RenderMessage(System.IFormatProvider)"/> output is added to the
        /// created <see cref="ITelemetry"/> Properties using the <see cref="TelemetryPropertiesRenderedMessage"/> key.</param>
        /// <param name="includeMessageTemplateAsProperty">if set to <c>true</c> the <see cref="LogEvent.MessageTemplate"/> is added to the
        /// created <see cref="ITelemetry"/> Properties using the <see cref="TelemetryPropertiesMessageTemplate"/> key.</param>
        /// <returns>An <see cref="TraceTelemetry"/> for the <paramref name="logEvent"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="logEvent" /> is <see langword="null" />.</exception>
        public static TraceTelemetry ToDefaultTraceTelemetry(
            this LogEvent logEvent,
            IFormatProvider formatProvider,
            bool includeLogLevelAsProperty = false,
            bool includeRenderedMessageAsProperty = false,
            bool includeMessageTemplateAsProperty = false,
            bool useJsonFormatter = false)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var renderedMessage = logEvent.RenderMessage(formatProvider);

            var telemetry = new TraceTelemetry(renderedMessage)
            {
                Timestamp = logEvent.Timestamp,
                SeverityLevel = logEvent.Level.ToSeverityLevel()
            };

            // write logEvent's .Properties to the AI one
            logEvent.ForwardPropertiesToTelemetryProperties(telemetry,
                formatProvider,
                includeLogLevelAsProperty,
                includeRenderedMessageAsProperty,
                includeMessageTemplateAsProperty);

            return telemetry;
        }
    }
}