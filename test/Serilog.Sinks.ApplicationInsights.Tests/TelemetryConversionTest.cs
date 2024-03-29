﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests;

public class TelemetryConversionTest : ApplicationInsightsTest
{
    public TelemetryConversionTest() : base(new CustomConverter())
    {
    }

    [Fact]
    public void Converter_triggers()
    {
        Logger.Information("test");

        Assert.Single(SubmittedTelemetry);
    }

    [Fact]
    public void Convert_to_two_traces()
    {
        Logger.Information("two");

        Assert.Equal(expected: 2, SubmittedTelemetry.Count(t => t is TraceTelemetry tt && tt.Message == "two"));
    }

    class CustomConverter : TraceTelemetryConverter
    {
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var tt = base.Convert(logEvent, formatProvider);

            if (logEvent.MessageTemplate.Text == "two")
            {
                yield return tt.First();
                yield return tt.First();
            }
            else
            {
                yield return tt.First();
            }
        }
    }
}