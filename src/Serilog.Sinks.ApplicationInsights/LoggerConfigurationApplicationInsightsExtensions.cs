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

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace Serilog
{

    /// <summary>
    /// Adds the WriteTo.ApplicationInsights() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationApplicationInsightsExtensions
    {
        /// <summary>
        /// Adds a Serilog sink that writes <see cref="LogEvent">log events</see> to Microsoft Application Insights 
        /// using a custom <see cref="ITelemetry"/> converter / constructor.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="telemetryConfiguration">Required Application Insights configuration settings.</param>
        /// <param name="telemetryConverter">Required telemetry converter.</param>
        /// <returns></returns>
        public static LoggerConfiguration ApplicationInsights(
            this LoggerSinkConfiguration loggerConfiguration,
            TelemetryConfiguration telemetryConfiguration,
            ITelemetryConverter telemetryConverter)
        {
            var client = new TelemetryClient(telemetryConfiguration ?? TelemetryConfiguration.Active);

            return loggerConfiguration.Sink(new ApplicationInsightsSink(client, telemetryConverter));
        }

        /// <summary>
        /// Adds a Serilog sink that writes <see cref="LogEvent">log events</see> to Microsoft Application Insights 
        /// using a custom <see cref="ITelemetry"/> converter / constructor. Only use in rare cases when your application doesn't
        /// have already constructed AI telemetry configuration, which is extremely rare.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">Required Application Insights key.</param>
        /// <param name="telemetryConverter">Required telemetry converter.</param>
        /// <returns></returns>
        public static LoggerConfiguration ApplicationInsights(
            this LoggerSinkConfiguration loggerConfiguration,
            string instrumentationKey,
            ITelemetryConverter telemetryConverter)
        {
            var client = new TelemetryClient();

            if (!string.IsNullOrWhiteSpace(instrumentationKey))
            {
                client.InstrumentationKey = instrumentationKey;
            }

            return loggerConfiguration.Sink(new ApplicationInsightsSink(client, telemetryConverter));
        }
    }
}
