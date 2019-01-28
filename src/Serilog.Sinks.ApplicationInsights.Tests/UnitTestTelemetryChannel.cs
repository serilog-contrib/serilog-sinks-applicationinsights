using Microsoft.ApplicationInsights.Channel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Sinks.ApplicationInsights.Tests
{
    class UnitTestTelemetryChannel : ITelemetryChannel
    {
        public List<ITelemetry> SubmittedTelemetry { get; } = new List<ITelemetry>();

        public bool? DeveloperMode
        {
            get => false;
            set { }
        }

        public string EndpointAddress
        {
            get => null;
            set { }
        }

        public void Dispose()
        {

        }

        public void Flush()
        {

        }

        public void Send(ITelemetry item)
        {
            SubmittedTelemetry.Add(item);
        }
    }
}
