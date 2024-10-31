using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;

namespace Serilog.Sinks.ApplicationInsights.Tests;

public class UnitTestTelemetryChannel : ITelemetryChannel, IAsyncFlushable
{
    public List<ITelemetry> SubmittedTelemetry { get; } = new();
    public bool FlushCalled { get; private set; }
    public bool FlushAsyncCalled { get; private set; }

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
        FlushCalled = true;
    }
    
    public Task<bool> FlushAsync(CancellationToken cancellationToken)
    {
        FlushAsyncCalled = true;
        return Task.FromResult(true);
    }

    public void Send(ITelemetry item)
    {
        SubmittedTelemetry.Add(item);
    }
}