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
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
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
        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly TelemetryClient _telemetryClient;
        private readonly IFormatProvider _formatProvider;
        private readonly Action<LogEvent, IFormatProvider, ITelemetry, ISupportProperties> _logEventDataToTelemetryForwarder;

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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is being disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is being disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposing
        {
            get
            {
                return Interlocked.Read(ref _isDisposing) == 1;
            }
            protected set
            {
                Interlocked.Exchange(ref _isDisposing, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has been disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get
            {
                return Interlocked.Read(ref _isDisposed) == 1;
            }
            protected set
            {
                Interlocked.Exchange(ref _isDisposed, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets the format provider.
        /// </summary>
        /// <value>
        /// The format provider.
        /// </value>
        protected IFormatProvider FormatProvider
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _formatProvider;
            }
        }

        /// <summary>
        /// Gets the log event data to telemetry forwarder.
        /// </summary>
        /// <value>
        /// The log event data to telemetry forwarder.
        /// </value>
        protected Action<LogEvent, IFormatProvider, ITelemetry, ISupportProperties> LogEventDataToTelemetryForwarder
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _logEventDataToTelemetryForwarder;
            }
        }

        /// <summary>
        /// Holds the actual Application Insights TelemetryClient that will be used for logging.
        /// </summary>
        public TelemetryClient TelemetryClient
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _telemetryClient;
            }
        }

        /// <summary>
        /// Creates a sink that saves logs to the Application Insights account for the given <paramref name="telemetryClient" /> instance.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights <paramref name="telemetryClient" />.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <param name="logEventDataToTelemetryForwarder">The <see cref="LogEvent"/> data to AI <see cref="ITelemetry"/> forwarder
        /// provides control over what data of each <see cref="LogEvent"/> is sent to Application Insights, particularly the Message itself but also Properties.
        /// If none is provided, all properties are sent to AI (albeit flattened).
        /// </param>
        /// <exception cref="System.ArgumentNullException">telemetryClient</exception>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient" /> cannot be null</exception>
        protected ApplicationInsightsSink(
            TelemetryClient telemetryClient,
            IFormatProvider formatProvider = null,
            Action<LogEvent, IFormatProvider, ITelemetry, ISupportProperties> logEventDataToTelemetryForwarder = null)
        {
            if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");

            _telemetryClient = telemetryClient;
            _formatProvider = formatProvider;
            _logEventDataToTelemetryForwarder = logEventDataToTelemetryForwarder ?? DefaultLogEventDataToTelemetryForwarder;
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

            CheckForAndThrowIfDisposed();

            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception)
            {
                SeverityLevel = logEvent.Level.ToSeverityLevel(),
                HandledAt = ExceptionHandledAt.UserCode,
                Timestamp = logEvent.Timestamp
            };

            // write logEvent's .Properties to the AI one
            ForwardLogEventDataToTelemetry(logEvent, FormatProvider, exceptionTelemetry, exceptionTelemetry);

            TelemetryClient.TrackException(exceptionTelemetry);
        }

        /// <summary>
        /// Forwards the <see cref="LogEvent"/> data to the <paramref name="telemetry"/> and its <paramref name="telemetryProperties"/>.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="telemetry">The telemetry itself.</param>
        /// <param name="telemetryProperties">The <paramref name="telemetry"/> properties.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="logEvent" />, <paramref name="formatProvider" />, <paramref name="telemetry" /> or <paramref name="telemetryProperties" /> is null.</exception>
        protected void ForwardLogEventDataToTelemetry(LogEvent logEvent, IFormatProvider formatProvider, ITelemetry telemetry, ISupportProperties telemetryProperties)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");
            if (telemetry == null) throw new ArgumentNullException("telemetry");
            if (telemetryProperties == null) throw new ArgumentNullException("telemetryProperties");
            if (formatProvider == null) throw new ArgumentNullException("formatProvider");

            try
            {
                LogEventDataToTelemetryForwarder.Invoke(logEvent, formatProvider, telemetry, telemetryProperties);
            }
            catch (TargetInvocationException targetInvocationException)
            {
                // rethrow original exception (inside the TargetInvocationException)
                ExceptionDispatchInfo.Capture(targetInvocationException).Throw();
            }
        }


        /// <summary>
        /// Default <see cref="LogEvent"/> data forwarder to the <paramref name="telemetry"/> and its <paramref name="telemetryProperties"/>.
        /// This forwards the the log level, rendered message, message template and all <paramref name="logEvent"/> properties to the telemetry.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="telemetry">The telemetry itself.</param>
        /// <param name="telemetryProperties">The telemetry properties.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="logEvent" />, <paramref name="formatProvider" />, <paramref name="telemetry" /> or <paramref name="telemetryProperties" /> is null.</exception>
        protected virtual void DefaultLogEventDataToTelemetryForwarder(LogEvent logEvent, IFormatProvider formatProvider, ITelemetry telemetry, ISupportProperties telemetryProperties)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");
            if (telemetry == null) throw new ArgumentNullException("telemetry");
            if (telemetryProperties == null) throw new ArgumentNullException("telemetryProperties");
            if (formatProvider == null) throw new ArgumentNullException("formatProvider");

            var renderedMessage = logEvent.RenderMessage(formatProvider);

            telemetryProperties.Properties.Add(TelemetryPropertiesLogLevel, logEvent.Level.ToString());
            telemetryProperties.Properties.Add(TelemetryPropertiesMessageTemplate, logEvent.MessageTemplate.Text);
            telemetryProperties.Properties.Add(TelemetryPropertiesRenderedMessage, renderedMessage);

            foreach (var property in logEvent.Properties.Where(property => property.Value != null && !telemetryProperties.Properties.ContainsKey(property.Key)))
            {
                ApplicationInsightsPropertyFormatter.WriteValue(property.Key, property.Value, telemetryProperties.Properties);
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
        /// Checks whether this instance has been disposed and if so, throws an <see cref="ObjectDisposedException"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        protected void CheckForAndThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManagedResources"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (IsDisposing || IsDisposed)
                return;

            try
            {
                IsDisposing = true;

                // we only have managed resources to dispose of
                if (disposeManagedResources)
                {
                    // free managed resources
                    if (TelemetryClient != null)
                    {
                        TelemetryClient.Flush();
                    }
                }

                // no unmanaged resources are to be disposed
            }
            finally
            {
                // but the flags need to be set in either case

                IsDisposed = true;
                IsDisposing = false;
            }
        }

        #endregion Implementation of IDisposable
    }
}
