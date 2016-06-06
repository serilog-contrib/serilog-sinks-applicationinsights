# Serilog.Sinks.ApplicationInsights

A sink for Serilog that writes events to Microsoft Application Insights.
 
[![Build status](https://ci.appveyor.com/api/projects/status/ccgd7k98kbmifl5v/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-applicationinsights/branch/master) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.ApplicationInsights.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights/)

This Sink can write Serilog `LogEvent` messages to Application Insights as `EventTelemetry`:

```csharp
var log = new LoggerConfiguration()
    .WriteTo
	.ApplicationInsightsEvents("<MyApplicationInsightsInstrumentationKey>")
    .CreateLogger();
```


.. or as `TraceTelemetry`:


```csharp
var log = new LoggerConfiguration()
    .WriteTo
	.ApplicationInsightsTraces("<MyApplicationInsightsInstrumentationKey>")
    .CreateLogger();
```

`LogEvent` instances that have Exceptions are always sent as Exceptions to AI though.

* [Serilog Documentation](https://github.com/serilog/serilog/wiki)

## How, When and Why to Flush Messages Manually

### Or: Where did my Messages go?

As explained by the [Application Insights documentation](https://azure.microsoft.com/en-us/documentation/articles/app-insights-api-custom-events-metrics/#flushing-data), the default behaviour of the AI client is to buffer messages and send them to AI in batches whenever the client seems fit. However, this may lead to lost messages when your application terminates while there are still unsent messages in said buffer.

You can either use Persistent Channels (see below) or control when AI shall flush its messages, for example when your application closes:

1.) Create a custom `TelemetryClient` and hold on to it in a field or property:

```csharp
// private TelemetryClient _telemetryClient;

// ...
_telemetryClient = new TelemetryClient()
            {
                InstrumentationKey = "<My AI Instrumentation Key>"
            };
```

2.) Use that custom `TelemetryClient` to initialize the Sink:

```csharp
var log = new LoggerConfiguration()
    .WriteTo
	.ApplicationInsightsEvents(telemetryClient)
    .CreateLogger();
```

3.) Call .Flush() on the TelemetryClient whenever you deem necessary, i.e. Application Shutdown:

```csharp
_telemetryClient.Flush();

// The AI Documentation mentions that calling .Flush() *can* be asynchronous and non-blocking so
// depending on the underlying Channel to AI you might want to wait some time
// specific to your application and its connectivity constraints for the flush to finish.

await Task.Delay(1000);

// or 

System.Threading.Thread.Sleep(1000);

```

## Using AI Persistent Channels
By default the Application Insights client and therefore also this Sink use an in-memory buffer of messages which are sent out periodically whenever the AI client deems necessary. This may lead to unexpected behaviour upon process termination, particularly [not all of your logged messages may have been sent and therefore be lost](https://github.com/serilog/serilog-sinks-applicationinsights/pull/9).

Besides flushing the messages manually (see above), you can also use a custom `ITelemetryChannel` such as the [Persistent Channel(s)](https://azure.microsoft.com/en-us/documentation/articles/app-insights-windows-services/#persistence-channel) one with this Sink and thereby *not* lose messages, i.e. like this:

1.) Add the [Microsoft.ApplicationInsights.PersistenceChannel](https://www.nuget.org/packages/Microsoft.ApplicationInsights.PersistenceChannel) to your project

2.) Create a `TelemetryConfiguration` using the Persistence Channel:

```csharp
var configuration = new TelemetryConfiguration()
            {
                InstrumentationKey = "<My AI Instrumentation Key>",
                TelemetryChannel = new PersistenceChannel()
            };
```

3.) Use that custom `TelemetryConfiguration` to initialize the Sink:

```csharp
var log = new LoggerConfiguration()
    .WriteTo
	.ApplicationInsightsEvents(configuration)
    .CreateLogger();
```

Copyright &copy; 2016 Serilog Contributors - Provided under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html).
