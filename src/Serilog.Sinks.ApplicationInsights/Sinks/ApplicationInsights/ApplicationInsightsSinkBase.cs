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
    public abstract class ApplicationInsightsSinkBase : ILogEventSink, IDisposable
    {
        private long _isDisposing = 0;
        private long _isDisposed = 0;

        private readonly TelemetryClient _telemetryClient;
        private readonly IFormatProvider _formatProvider;
        private readonly Func<LogEvent, IFormatProvider, ITelemetry> _logEventToTelemetryConverter;

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
        /// Gets the <see cref="LogEvent"/> to <see cref="ITelemetry"/> converter.
        /// </summary>
        /// <value>
        /// The log event to telemetry converter.
        /// </value>
        protected Func<LogEvent, IFormatProvider, ITelemetry> LogEventToTelemetryConverter
        {
            get
            {
                CheckForAndThrowIfDisposed();

                return _logEventToTelemetryConverter;
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
        /// <param name="logEventToTelemetryConverter">The <see cref="LogEvent"/> to <see cref="ITelemetry"/> converter.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null for default provider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="telemetryClient" /> cannot be null</exception>
        protected ApplicationInsightsSinkBase(
            TelemetryClient telemetryClient,
            Func<LogEvent, IFormatProvider, ITelemetry> logEventToTelemetryConverter,
            IFormatProvider formatProvider = null)
        {
            if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
            if (logEventToTelemetryConverter == null) throw new ArgumentNullException("logEventToTelemetryConverter");

            _telemetryClient = telemetryClient;
            _formatProvider = formatProvider;
            _logEventToTelemetryConverter = logEventToTelemetryConverter;
        }

        #region AI specifc Helper methods

        /// <summary>
        /// Hands over the <paramref name="telemetry" /> to the AI telemetry client.
        /// </summary>
        /// <param name="telemetry">The telemetry.</param>
        /// <exception cref="System.ArgumentNullException">telemetry</exception>
        protected virtual void TrackTelemetry(ITelemetry telemetry)
        {
            if (telemetry == null) throw new ArgumentNullException("telemetry");

            CheckForAndThrowIfDisposed();

            // the .Track() method is save to use (even though documented otherwise)
            // see https://github.com/Microsoft/ApplicationInsights-dotnet/issues/244
            TelemetryClient.Track(telemetry);
        }

        #endregion AI specifc Helper methods

        #region Implementation of ILogEventSink

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        /// <exception cref="TargetInvocationException">A delegate callback throws an exception.</exception>
        public virtual void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            CheckForAndThrowIfDisposed();

            try
            {
                var telemetry = LogEventToTelemetryConverter.Invoke(logEvent, FormatProvider);

                // if 'null' is returned (& we therefore there's nothing to track), the logEvent is basically skipped
                if (telemetry != null)
                {
                    TrackTelemetry(telemetry);
                }
            }
            catch (TargetInvocationException targetInvocationException)
            {
                // rethrow original exception (inside the TargetInvocationException) if any
                if (targetInvocationException.InnerException != null)
                {
                    ExceptionDispatchInfo.Capture(targetInvocationException.InnerException).Throw();
                }
                else
                {
                    throw;
                }
            }
        }

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
