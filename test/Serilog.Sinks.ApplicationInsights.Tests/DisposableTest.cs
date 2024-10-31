using System;
using System.Threading.Tasks;
using Xunit;

namespace Serilog.Sinks.ApplicationInsights.Tests;

public class DisposableTest : ApplicationInsightsTest
{
    [Fact]
    public void Flush_is_called_on_channel_when_logger_is_disposed()
    {
        ((IDisposable)Logger).Dispose();
        Assert.True(Channel.FlushCalled, "Channel.Flush was not called");
    }
    
#if NET6_0_OR_GREATER
    [Fact]
    public async Task FlushAsync_is_called_on_channel_when_logger_is_disposed_asynchronously()
    {
        await ((IAsyncDisposable)Logger).DisposeAsync();
        Assert.True(Channel.FlushAsyncCalled, "Channel.FlushAsync was not called");
    }
#endif
}