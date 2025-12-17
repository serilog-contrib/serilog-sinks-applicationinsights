// SPDX-FileCopyrightText: 2025 Serilog Contributors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Enrichers;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests.Enrichers;

public class ActivityBaggageEnricherTests
{
    [Fact]
    public void Single_baggage_value_is_enriched()
    {
        ActivityBaggageEnricher enricher = new();
        using Activity activity = new("TestActivity");
        string baggageName = Guid.NewGuid().ToString("N");
        string baggageValue = Guid.NewGuid().ToString("N");
        activity.AddBaggage(baggageName, baggageValue);
        activity.Start();
        LogEvent logEvent = new(DateTimeOffset.Now, LogEventLevel.Information, null, MessageTemplate.Empty, []);

        enricher.Enrich(logEvent, TestLogEventPropertyFactory.Instance);

        bool hasProperty = logEvent.Properties.TryGetValue("Baggage", out LogEventPropertyValue property);
        Assert.True(hasProperty);
        Assert.NotNull(property);
        Assert.IsType<StructureValue>(property);
        StructureValue scalarValue = (StructureValue)property;
        Assert.Single(scalarValue.Properties);
        LogEventProperty valueProperty = scalarValue.Properties[0];
        Assert.Equal(baggageName, valueProperty.Name);
        Assert.IsType<ScalarValue>(valueProperty.Value);
        ScalarValue scalarValueInner = (ScalarValue)valueProperty.Value;
        Assert.Equal(baggageValue, scalarValueInner.Value);
    }

    [Fact]
    public void Multiple_baggage_values_are_enriched()
    {
        Random random = new();
        Dictionary<string, string> baggageItems = GenerateRandomBaggageItems(10);

        ActivityBaggageEnricher enricher = new();
        using Activity activity = new("TestActivity");
        foreach (var item in baggageItems)
        {
            activity.AddBaggage(item.Key, item.Value);
        }

        activity.Start();
        LogEvent logEvent = new(DateTimeOffset.Now, LogEventLevel.Information, null, MessageTemplate.Empty, []);

        enricher.Enrich(logEvent, TestLogEventPropertyFactory.Instance);

        bool hasProperty = logEvent.Properties.TryGetValue("Baggage", out LogEventPropertyValue property);
        Assert.True(hasProperty);
        Assert.NotNull(property);
        Assert.IsType<StructureValue>(property);
        StructureValue scalarValue = (StructureValue)property;
        Assert.Equal(baggageItems.Count, scalarValue.Properties.Count);

        foreach (var item in baggageItems)
        {
            LogEventProperty valueProperty = scalarValue.Properties.FirstOrDefault(p => p.Name == item.Key);
            Assert.NotNull(valueProperty);
            Assert.IsType<ScalarValue>(valueProperty.Value);
            ScalarValue scalarValueInner = (ScalarValue)valueProperty.Value;
            Assert.Equal(item.Value, scalarValueInner.Value);
        }
    }

    private static Dictionary<string, string> GenerateRandomBaggageItems(int count)
    {
        Dictionary<string, string> baggageItems = new(count);
        for (int i = 0; i < count; i++)
        {
            string key = Guid.NewGuid().ToString("N");
            string value = Guid.NewGuid().ToString("N");
            baggageItems[key] = value;
        }

        return baggageItems;
    }
}
