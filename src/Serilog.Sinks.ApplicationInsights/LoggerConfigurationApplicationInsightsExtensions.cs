﻿// Copyright 2016 Serilog Contributors
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
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Serilog;

/// <summary>
///     Adds the WriteTo.ApplicationInsights() extension method to <see cref="LoggerConfiguration" />.
/// </summary>
public static class LoggerConfigurationApplicationInsightsExtensions
{
    /// <summary>
    ///     Adds a Serilog sink that writes <see cref="LogEvent">log events</see> to Microsoft Application Insights
    ///     using a custom <see cref="ITelemetry" /> converter / constructor.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="telemetryConfiguration">Required Application Insights configuration settings.</param>
    /// <param name="telemetryConverter">Required telemetry converter.</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
    /// <param name="levelSwitch">Logging level switch for this sink</param>
    /// <returns></returns>
    public static LoggerConfiguration ApplicationInsights(
        this LoggerSinkConfiguration loggerConfiguration,
        TelemetryConfiguration telemetryConfiguration,
        ITelemetryConverter telemetryConverter,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch levelSwitch = null)
    {
#pragma warning disable CS0618
        var client = new TelemetryClient(telemetryConfiguration ?? TelemetryConfiguration.Active);
#pragma warning restore CS0618

        return loggerConfiguration.Sink(new ApplicationInsightsSink(client, telemetryConverter),
            restrictedToMinimumLevel, levelSwitch);
    }

    /// <summary>
    ///     Adds a Serilog sink that writes <see cref="LogEvent">log events</see> to Microsoft Application Insights
    ///     using the active <see cref="TelemetryConfiguration" />
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="telemetryConverter">Required telemetry converter.</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
    /// <param name="levelSwitch">Logging level switch for this sink</param>
    /// <returns></returns>
    public static LoggerConfiguration ApplicationInsights(
        this LoggerSinkConfiguration loggerConfiguration,
        ITelemetryConverter telemetryConverter,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch levelSwitch = null)
    {
#pragma warning disable CS0618
        var client = new TelemetryClient(TelemetryConfiguration.Active);
#pragma warning restore CS0618

        return loggerConfiguration.Sink(new ApplicationInsightsSink(client, telemetryConverter),
            restrictedToMinimumLevel, levelSwitch);
    }

    /// <summary>
    ///     Adds a Serilog sink that writes <see cref="LogEvent">log events</see> to Microsoft Application Insights
    ///     using a custom <see cref="ITelemetry" /> converter / constructor.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="telemetryClient">Required Application Insights telemetry client.</param>
    /// <param name="telemetryConverter">Required telemetry converter.</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
    /// <param name="levelSwitch">Logging level switch for this sink</param>
    /// <returns></returns>
    public static LoggerConfiguration ApplicationInsights(
        this LoggerSinkConfiguration loggerConfiguration,
        TelemetryClient telemetryClient,
        ITelemetryConverter telemetryConverter,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch levelSwitch = null)
    {
        return loggerConfiguration.Sink(new ApplicationInsightsSink(telemetryClient, telemetryConverter),
            restrictedToMinimumLevel, levelSwitch);
    }


    /// <summary>
    ///     Adds a Serilog sink that writes <see cref="LogEvent">log events</see> to Microsoft Application Insights
    ///     using a custom <see cref="ITelemetry" /> converter / constructor. Only use in rare cases when your application
    ///     doesn't
    ///     have already constructed AI telemetry configuration, which is extremely rare.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="connectionString">Required Application Insights connection string.</param>
    /// <param name="telemetryConverter">Required telemetry converter.</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
    /// <param name="levelSwitch">Logging level switch for this sink</param>
    /// <returns></returns>
    public static LoggerConfiguration ApplicationInsights(
        this LoggerSinkConfiguration loggerConfiguration,
        string connectionString,
        ITelemetryConverter telemetryConverter,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch levelSwitch = null)
    {
        var config = TelemetryConfiguration.CreateDefault();
        if (!string.IsNullOrWhiteSpace(connectionString)) config.ConnectionString = connectionString;
#pragma warning disable CS0618
        var client = new TelemetryClient(config);
#pragma warning restore CS0618

        return loggerConfiguration.Sink(new ApplicationInsightsSink(client, telemetryConverter),
            restrictedToMinimumLevel, levelSwitch);
    }
}