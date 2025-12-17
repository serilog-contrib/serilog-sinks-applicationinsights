// SPDX-FileCopyrightText: 2025 Serilog Contributors
// SPDX-License-Identifier: Apache-2.0

using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.Tests.Enrichers;

internal class TestLogEventPropertyFactory : ILogEventPropertyFactory
{
    public static TestLogEventPropertyFactory Instance { get; } = new();

    public LogEventProperty CreateProperty(string name, object value, bool destructureObjects = false)
        => new(name, new ScalarValue(value));
}
