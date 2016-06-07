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
using System.Threading;
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
        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly TelemetryClient _telemetryClient;

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
        /// The format provider
        /// </summary>
        protected IFormatProvider FormatProvider { get; private set; }

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
        /// Creates a sink that saves logs to the Application Insights account for the given <paramref name="telemetryClient"/> instance.
        /// </summary>
        /// <param name="telemetryClient">Required Application Insights <paramref name="telemetryClient"/>.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient"/> cannot be null</exception>
        protected ApplicationInsightsSink(TelemetryClient telemetryClient, IFormatProvider formatProvider = null)
        {
            if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");

            _telemetryClient = telemetryClient;
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

            CheckForAndThrowIfDisposed();

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
            telemetry.Properties.Add("MessageTemplate", logEvent.MessageTemplate.Text);
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
