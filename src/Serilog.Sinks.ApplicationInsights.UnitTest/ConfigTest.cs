using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Core;
using Serilog.Events;
using Serilog.ExtensionMethods;
using Serilog.Parsing;

namespace Serilog.Sinks.ApplicationInsights.UnitTest
{
    public class LogToTelemetryTestConverter : ILogEventToTelemetryConverter
    {
        public ITelemetry Invoke(LogEvent logEvent, IFormatProvider formatProvider)
        {
            Console.WriteLine($"write event to telemetry.{logEvent.MessageTemplate} as {logEvent.Level}");
            return logEvent.ToDefaultTelemetry<EventTelemetry>(formatProvider);
        }
    }
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void LoadFromConfigTest()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();


            logger.ForContext<ConfigTest>().Information("Hello, world!");
            logger.ForContext<ConfigTest>().Error("Hello, world!");
            logger.ForContext(Constants.SourceContextPropertyName, "Microsoft").Warning("Hello, world!");
            logger.ForContext(Constants.SourceContextPropertyName, "Microsoft").Error("Hello, world!");
            logger.ForContext(Constants.SourceContextPropertyName, "MyApp.Something.Tricky").Verbose("Hello, world!");
            logger.Fatal(new Exception("blub"), "Fatal Exception");
        }

        [TestMethod]
        [DataRow(typeof(RequestTelemetry))]
        [DataRow(typeof(PageViewTelemetry))]
        [DataRow(typeof(MetricTelemetry))]
        [DataRow(typeof(DependencyTelemetry))]
        [DataRow(typeof(AvailabilityTelemetry))]
        [ExpectedException(typeof(NotImplementedException))]
        public void NotImplementedDefaultTelemetryConvertersTest(Type notImplementedType)
        {
            var log = new LogEvent(new DateTimeOffset(), LogEventLevel.Debug, null, new MessageTemplate("test", new List<MessageTemplateToken>()), new List<LogEventProperty>());
            var parameters = new object[]{ log, null, null, null, null };
            try
            {
                typeof(LogEventExtensions).GetMethod(nameof(LogEventExtensions.ToDefaultTelemetry)).MakeGenericMethod(notImplementedType).Invoke(log, parameters);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            } 
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void NotSupportedTelemetryTest()
        {
            var log = new LogEvent(new DateTimeOffset(), LogEventLevel.Debug, null, new MessageTemplate("test", new List<MessageTemplateToken>()), new List<LogEventProperty>());
            log.ToDefaultTelemetry<SessionStateTelemetry>(null);
        }
    }
}
