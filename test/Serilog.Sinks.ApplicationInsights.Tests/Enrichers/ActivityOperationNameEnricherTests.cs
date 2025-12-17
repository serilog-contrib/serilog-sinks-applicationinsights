// SPDX-FileCopyrightText: 2025 Serilog Contributors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Enrichers;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.Enrichers;

public class ActivityOperationNameEnricherTests
{
    [Fact]
    public void Operation_name_is_enriched()
    {
        ActivityOperationNameEnricher enricher = new();
        string operationName = Guid.NewGuid().ToString("N");
        using Activity activity = new(operationName);
        activity.Start();
        LogEvent logEvent = new(DateTimeOffset.Now, LogEventLevel.Information, null, MessageTemplate.Empty, []);

        enricher.Enrich(logEvent, TestLogEventPropertyFactory.Instance);

        bool hasProperty = logEvent.Properties.TryGetValue("OperationName", out LogEventPropertyValue property);
        Assert.True(hasProperty);
        Assert.NotNull(property);
        Assert.IsType<ScalarValue>(property);
        ScalarValue scalarValue = (ScalarValue)property;
        Assert.Equal(operationName, scalarValue.Value);
    }
}
