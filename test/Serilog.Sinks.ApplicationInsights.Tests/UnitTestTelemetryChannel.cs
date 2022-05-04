using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;

namespace Serilog.Sinks.ApplicationInsights.Tests;

class UnitTestTelemetryChannel : ITelemetryChannel
{
    public List<ITelemetry> SubmittedTelemetry { get; } = new();

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