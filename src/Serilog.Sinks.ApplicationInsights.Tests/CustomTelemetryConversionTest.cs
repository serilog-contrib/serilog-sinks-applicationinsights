using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Events;
using Serilog.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    public class CustomTelemetryConversionTest : ApplicationInsightsTest
    {
        public CustomTelemetryConversionTest() : base((Func<LogEvent, IFormatProvider, ITelemetry>)ConvertCustom)
        {

        }

        [Fact]
        public void LogCustom()
        {
            Logger.Information("test");

            Assert.Equal("converted!", LastSubmittedTraceTelemetry.Message);
        }

        private static ITelemetry ConvertCustom(LogEvent logEvent, IFormatProvider formatProvider)
        {
            // first create a default TraceTelemetry using the sink's default logic
            // .. but without the log level, and (rendered) message (template) included in the Properties
            var telemetry = GetTelemetry(logEvent, formatProvider);

            // then go ahead and post-process the telemetry's context to contain the user id as desired
            if (logEvent.Properties.ContainsKey("UserId"))
            {
                telemetry.Context.User.Id = logEvent.Properties["UserId"].ToString();
            }
            // post-process the telemetry's context to contain the operation id
            if (logEvent.Properties.ContainsKey("operation_Id"))
            {
                telemetry.Context.Operation.Id = logEvent.Properties["operation_Id"].ToString();
            }
            // post-process the telemetry's context to contain the operation parent id
            if (logEvent.Properties.ContainsKey("operation_parentId"))
            {
                telemetry.Context.Operation.ParentId = logEvent.Properties["operation_parentId"].ToString();
            }
            // typecast to ISupportProperties so you can manipulate the properties as desired
            ISupportProperties propTelematry = (ISupportProperties)telemetry;

            // find redundent properties
            var removeProps = new[] { "UserId", "operation_parentId", "operation_Id" };
            removeProps = removeProps.Where(prop => propTelematry.Properties.ContainsKey(prop)).ToArray();

            foreach (var prop in removeProps)
            {
                // remove redundent properties
                propTelematry.Properties.Remove(prop);
            }

            var tpcb = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;


            return telemetry;
        }

        private static ITelemetry GetTelemetry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            if (logEvent.Exception != null)
            {
                // Exception telemetry
                return logEvent.ToDefaultExceptionTelemetry(
                formatProvider,
                includeLogLevelAsProperty: false,
                includeRenderedMessageAsProperty: false,
                includeMessageTemplateAsProperty: false);
            }
            else
            {
                // default telemetry
                return logEvent.ToDefaultTraceTelemetry(
                formatProvider,
                includeLogLevelAsProperty: false,
                includeRenderedMessageAsProperty: false,
                includeMessageTemplateAsProperty: false);
            }
        }
    }
}
