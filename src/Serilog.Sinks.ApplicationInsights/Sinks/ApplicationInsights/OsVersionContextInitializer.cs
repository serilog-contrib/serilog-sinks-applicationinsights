using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// An <see cref="IContextInitializer"/> implementation that appends the <see cref="Environment.OSVersion"/> to an Application Insights <see cref="TelemetryContext"/>
    /// </summary>
    public class OsVersionContextInitializer : IContextInitializer
    {
        /// <summary>
        /// Builds the (lazy) Value for the .OperatingSystem property for the <see cref="TelemetryContext.Device"/> information.
        /// </summary>
        private readonly Lazy<string> _osVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <param name="osVersion">The os version.</param>
        public OsVersionContextInitializer(string osVersion = null)
        {
            _osVersion = new Lazy<string>(() => string.IsNullOrWhiteSpace(osVersion)
                ? (Environment.OSVersion != null
                    ? Environment.OSVersion.ToString()
                    : string.Empty)
                : osVersion);
        }

        #region Implementation of IContextInitializer

        /// <summary>
        /// Initializes the given <see cref="T:Microsoft.ApplicationInsights.DataContracts.TelemetryContext"/>.
        /// </summary>
        public void Initialize(TelemetryContext context)
        {
            if (context == null)
                return;

            if (string.IsNullOrWhiteSpace(context.Device.OperatingSystem))
                context.Device.OperatingSystem = _osVersion.Value;
        }

        #endregion
    }
}