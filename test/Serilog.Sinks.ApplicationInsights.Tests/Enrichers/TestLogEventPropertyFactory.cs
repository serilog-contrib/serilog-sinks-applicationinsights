// SPDX-FileCopyrightText: 2025 Serilog Contributors
// SPDX-License-Identifier: Apache-2.0

using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.Tests.Enrichers;

internal class TestLogEventPropertyFactory : ILogEventPropertyFactory
{
    public static TestLogEventPropertyFactory Instance { get; } = new();

    public LogEventProperty CreateProperty(string name, object value, bool destructureObjects = false)
    {
        LogEventPropertyValue logEventPropertyValue;
        if (destructureObjects && value is IEnumerable<LogEventProperty> logEventProperties)
        {
            logEventPropertyValue = new StructureValue(logEventProperties);
        }
        else
        {
            logEventPropertyValue = new ScalarValue(value);
        }

        return new LogEventProperty(name, logEventPropertyValue);
    }
}
