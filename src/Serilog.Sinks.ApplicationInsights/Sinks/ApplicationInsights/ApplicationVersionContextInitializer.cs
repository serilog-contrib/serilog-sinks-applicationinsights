using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// An <see cref="IContextInitializer"/> implementation that appends the <see cref="Environment.OSVersion"/> to an Application Insights <see cref="TelemetryContext"/>
    /// </summary>
    public class ApplicationVersionContextInitializer : IContextInitializer
    {
        /// <summary>
        /// Gets or sets the application version.
        /// </summary>
        /// <value>
        /// The application version.
        /// </value>
        private readonly string _applicationVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationVersionContextInitializer"/> class.
        /// </summary>
        /// <param name="applicationVersion">The application version.</param>
        public ApplicationVersionContextInitializer(string applicationVersion)
        {
            if (applicationVersion == null) throw new ArgumentNullException("applicationVersion");

            _applicationVersion = applicationVersion;
        }

        #region Implementation of IContextInitializer

        /// <summary>
        /// Initializes the given <see cref="T:Microsoft.ApplicationInsights.DataContracts.TelemetryContext"/>.
        /// </summary>
        public void Initialize(TelemetryContext context)
        {
            if (context == null)
                return;

            if (string.IsNullOrWhiteSpace(context.Component.Version))
                context.Component.Version = _applicationVersion;
        }

        #endregion
    }
}