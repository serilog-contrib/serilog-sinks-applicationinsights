using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.Formatters;

namespace Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters
{
    /// <summary>
    /// Base class for telemetry converters
    /// </summary>
    public abstract class TelemetryConverterBase : ITelemetryConverter
    {
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

        /// <summary>
        /// Property that is when included in log context, will be pushed out as AI operation ID.
        /// </summary>
        public const string OperationIdProperty = "operationId";

        public virtual IValueFormatter ValueFormatter { get; }

        public abstract IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider);

        /// <summary>
        /// Creates an instance of <see cref="TelemetryConverterBase"/> using default value formatter (<see cref="ApplicationInsightsJsonValueFormatter"/>).
        /// </summary>
        public TelemetryConverterBase()
        {
            ValueFormatter = new ApplicationInsightsJsonValueFormatter();
        }

        public virtual ExceptionTelemetry ToExceptionTelemetry(
            LogEvent logEvent,
            IFormatProvider formatProvider)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (logEvent.Exception == null) throw new ArgumentException("Must have an Exception", nameof(logEvent));

            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception) {
                SeverityLevel = ToSeverityLevel(logEvent.Level),
                Timestamp = logEvent.Timestamp
            };

            // write logEvent's .Properties to the AI one
            ForwardPropertiesToTelemetryProperties(logEvent, exceptionTelemetry, formatProvider);

            return exceptionTelemetry;
        }

        public virtual void ForwardPropertiesToTelemetryProperties(LogEvent logEvent,
            ISupportProperties telemetryProperties,
            IFormatProvider formatProvider)
        {
            ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
                includeLogLevel: false,
                includeRenderedMessage: false,
                includeMessageTemplate: true);
        }

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
        public void ForwardPropertiesToTelemetryProperties(LogEvent logEvent,
            ISupportProperties telemetryProperties,
            IFormatProvider formatProvider,
            bool includeLogLevel,
            bool includeRenderedMessage,
            bool includeMessageTemplate)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (telemetryProperties == null) throw new ArgumentNullException(nameof(telemetryProperties));

            if (includeLogLevel)
            {
                telemetryProperties.Properties.Add(TelemetryPropertiesLogLevel, logEvent.Level.ToString());
            }

            if (includeRenderedMessage)
            {
                telemetryProperties.Properties.Add(TelemetryPropertiesRenderedMessage, logEvent.RenderMessage(formatProvider));
            }

            if (includeMessageTemplate)
            {
                telemetryProperties.Properties.Add(TelemetryPropertiesMessageTemplate, logEvent.MessageTemplate.Text);
            }

            if (telemetryProperties is ITelemetry telemetry && logEvent.Properties.TryGetValue(OperationIdProperty, out LogEventPropertyValue operationId))
            {
                telemetry.Context.Operation.Id = operationId.ToString().Trim('\"');
            }

            foreach (KeyValuePair<string, LogEventPropertyValue> property in logEvent.Properties.Where(property => property.Value != null && !telemetryProperties.Properties.ContainsKey(property.Key)))
            {
                ValueFormatter.Format(property.Key, property.Value, telemetryProperties.Properties);
            }
        }

        /// <summary>
        /// To the severity level.
        /// </summary>
        /// <param name="logEventLevel">The log event level.</param>
        /// <returns></returns>
        public SeverityLevel? ToSeverityLevel(LogEventLevel logEventLevel)
        {
            switch (logEventLevel)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                    return SeverityLevel.Verbose;
                case LogEventLevel.Information:
                    return SeverityLevel.Information;
                case LogEventLevel.Warning:
                    return SeverityLevel.Warning;
                case LogEventLevel.Error:
                    return SeverityLevel.Error;
                case LogEventLevel.Fatal:
                    return SeverityLevel.Critical;
            }

            return null;
        }
    }
}
