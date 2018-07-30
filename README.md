# Serilog.Sinks.ApplicationInsights

A sink for Serilog that writes events to Microsoft Application Insights.
 
[![Build status](https://ci.appveyor.com/api/projects/status/ccgd7k98kbmifl5v/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-applicationinsights/branch/master) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.ApplicationInsights.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights/)

This Sink comes with two main helper extensions that send Serilog `LogEvent` messages to Application Insights as either `EventTelemetry`:

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

Note: Whether you choose `EventTelemetry` or `TraceTelemetry `, if the LogEvent contains any exceptions it will always be sent as `ExceptionTelemetry`.

Additionally, you can also customize *whether* to send the LogEvents at all, if so *which type(s)* of Telemetry to send and also *what to send* (all or no LogEvent properties at all), via a bit more bare-metal set of overloads that take a  `ILogEventToTelemetryConverter logEventToTelemetryConverter` parameter, i.e. like this to send over MetricTelemetries:

```csharp
var log = new LoggerConfiguration()
    .WriteTo
	.ApplicationInsights("<MyApplicationInsightsInstrumentationKey>", new LogToTelemetryTestConverter())
    .CreateLogger();

// ....

public class LogToTelemetryTestConverter : ILogEventToTelemetryConverter
{
    public ITelemetry Invoke(LogEvent logEvent, IFormatProvider formatProvider)
    {
        var metricTelemetry = new MetricTelemetry(/* ...*/);
        // forward properties from logEvent or ignore them altogether...
        return metricTelemetry;
    }
}

```


.. or alternatively by using the built-in, default TraceTelemetry generation logic, but adapt the Telemetry's Context to include a UserId:


```csharp
public static void Main()
{
    var log = new LoggerConfiguration()
        .WriteTo
        .ApplicationInsights("<MyApplicationInsightsInstrumentationKey>", ConvertLogEventsToCustomTraceTelemetry)
        .CreateLogger();
}

public class LogToTelemetryTestConverter : ILogEventToTelemetryConverter
{
    public ITelemetry Invoke(LogEvent logEvent, IFormatProvider formatProvider)
    {
        // first create a default TraceTelemetry using the sink's default logic
        // .. but without the log level, and (rendered) message (template) included in the Properties
        var telemetry = logEvent.ToDefaultTelemetry<TraceTelemetry>(
            formatProvider,
            includeLogLevelAsProperty: false,
            includeRenderedMessageAsProperty: false,
            includeMessageTemplateAsProperty: false);

        // then go ahead and post-process the telemetry's context to contain the user id as desired
        if (logEvent.Properties.ContainsKey("UserId"))
        {
            telemetry.Context.User.Id = logEvent.Properties["UserId"].ToString();
        }

        // and remove the UserId from the Telemetry .Properties (we don't need redundancies)
        if (telemetry.Properties.ContainsKey("UserId"))
        {
            telemetry.Properties.Remove("UserId");
        }
	
        return telemetry;
    }
}
```

If you want to skip sending a particular LogEvent, just return `null` from your own converter method.


### How to configure application insights with appsettings.json

with new interface implementation its possible to configure Application Insights over the appsettings.json

```json
{
  "Serilog": {
    "Using": [ "[assembly fully qualified type name]" ],
    "WriteTo": [
      {
        "Name": "ApplicationInsights",
        "Args": {
          "restrictedToMinimumLevel": "Information",
          "instrumentationKey": "3c36...",
          "logEventToTelemetryConverter": "[Namespace].LogToTelemetryTestConverter, [assembly fully qualified type name]"
        }
      },
      {
        "Name": "ApplicationInsightsTraces",
        "Args": {
          "restrictedToMinimumLevel": "Information",
          "instrumentationKey": "3c36..."
        }
      }
    ]
  }
}
```

Example for C# initialization. For ConfigurationBuilder is `Microsoft.Extensions.Configuration.Json` Nuget package required

```csharp
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();


logger.Fatal(new Exception("blub"), "Fatal Exception");
```

this config will create two sinks. The first one with custom implemented `ILogEventToTelemetryConverter` and the second one with default `ILogEventToTelemetryConverter` implementation.


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

See also: [Serilog Documentation](https://github.com/serilog/serilog/wiki)
