﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Serilog.Debugging;
using Serilog.Events;

namespace Serilog.Sinks.ApplicationInsights.Formatters;

#pragma warning disable CS1591

public class ApplicationInsightsDottedValueFormatter : IValueFormatter
{
    static readonly IDictionary<Type, Action<string, object, IDictionary<string, string>>> LiteralWriters =
        new Dictionary
            <Type, Action<string, object, IDictionary<string, string>>> {
                { typeof(SequenceValue), (k, v, p) => WriteSequenceValue(k, (SequenceValue)v, p) },
                { typeof(DictionaryValue), (k, v, p) => WriteDictionaryValue(k, (DictionaryValue)v, p) },
                { typeof(StructureValue), (k, v, p) => WriteStructureValue(k, (StructureValue)v, p) },
                { typeof(ScalarValue), (k, v, p) => WriteValue(k, ((ScalarValue)v).Value, p) },
                { typeof(DateTime), (k, v, p) => AppendProperty(p, k, ((DateTime)v).ToString("o")) },
                { typeof(DateTimeOffset), (k, v, p) => AppendProperty(p, k, ((DateTimeOffset)v).ToString("o")) }, {
                    typeof(float),
                    (k, v, p) => AppendProperty(p, k, ((float)v).ToString("R", CultureInfo.InvariantCulture))
                }, {
                    typeof(double),
                    (k, v, p) => AppendProperty(p, k, ((double)v).ToString("R", CultureInfo.InvariantCulture))
                }
            };

    public void Format(string propertyName, LogEventPropertyValue propertyValue,
        IDictionary<string, string> properties)
    {
        WriteValue(propertyName, propertyValue, properties);
    }

    static void WriteStructureValue(string key, StructureValue structureValue,
        IDictionary<string, string> properties)
    {
        foreach (var eventProperty in structureValue.Properties)
            WriteValue(key + "." + eventProperty.Name, eventProperty.Value, properties);
    }

    static void WriteDictionaryValue(string key, DictionaryValue dictionaryValue,
        IDictionary<string, string> properties)
    {
        foreach (var eventProperty in dictionaryValue.Elements)
            WriteValue(key + "." + eventProperty.Key.Value, eventProperty.Value, properties);
    }

    static void WriteSequenceValue(string key, SequenceValue sequenceValue, IDictionary<string, string> properties)
    {
        var index = 0;
        foreach (var eventProperty in sequenceValue.Elements)
        {
            WriteValue(key + "." + index, eventProperty, properties);
            index++;
        }

        AppendProperty(properties, key + ".Count", index.ToString());
    }

    static void WriteValue(string key, object value, IDictionary<string, string> properties)
    {
        if (value == null || !LiteralWriters.TryGetValue(value.GetType(), out var writer))
        {
            AppendProperty(properties, key, value?.ToString());
            return;
        }

        writer(key, value, properties);
    }

    static void AppendProperty(IDictionary<string, string> propDictionary, string key, string value)
    {
        if (propDictionary.ContainsKey(key))
        {
            SelfLog.WriteLine("The key {0} is not unique after simplification. Ignoring new value {1}", key, value);
            return;
        }

        propDictionary.Add(key, value);
    }
}