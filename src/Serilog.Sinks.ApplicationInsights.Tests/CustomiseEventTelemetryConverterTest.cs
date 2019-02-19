using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public class CustomiseEventTelemetryConverterTest : ApplicationInsightsTest
    {
        public CustomiseEventTelemetryConverterTest() : base()
        {

        }

        private class IncludeRenderedMessageConverter : EventTelemetryConverter
        {
            public override void ForwardPropertiesToTelemetryProperties(LogEvent logEvent, ISupportProperties telemetryProperties, IFormatProvider formatProvider)
            {
                base.ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
                    includeLogLevel: false,
                    includeRenderedMessage: true,
                    includeMessageTemplate: false);
            }
        }
    }
}
