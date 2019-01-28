# Serilog.Sinks.ApplicationInsights

A sink for Serilog that writes events to Microsoft Application Insights.
 
[![Build status](https://ci.appveyor.com/api/projects/status/ccgd7k98kbmifl5v/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-applicationinsights/branch/master) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.ApplicationInsights.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights/)

This Sink comes with several helper extensions that send Serilog `LogEvent` messages to Application Insights as either `EventTelemetry` or `TraceTelemetry`.

The simplest way to configure Serilog to send data to a ApplicationInsights dashboard via Instrumentation key is:

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

However, you probably want to configure ApplicationInsights through a more traditional method, and just let Serilog use `TelemetryConfiguration.Active`. To do this in Startup.cs in an Asp.NET Core site:


```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<RequestTelemetryEnricherOptions>(Configuration);
    services.AddApplicationInsightsTelemetry();

    var log = new LoggerConfiguration()
        .WriteTo
	    .ApplicationInsightsTraces() // or .ApplicationInsightsEvents()
        .CreateLogger();

    // . . .
}
```

If you pass no paramaters to `ApplicationInsightsTraces()` or `ApplicationInsightsEvents()` it will create a `TelemetryClient` from `TelemetryConfiguration.Active`

**Note:** Whether you choose `EventTelemetry` or `TraceTelemetry `, if the LogEvent contains any exceptions it will always be sent as `ExceptionTelemetry`.

Additionally, you can also customize *whether* to send the LogEvents at all, if so *which type(s)* of Telemetry to send and also *what to send* (all or no LogEvent properties at all), via a bit more bare-metal set of overloads that take a  `Func<LogEvent, IFormatProvider, ITelemetry> logEventToTelemetryConverter` parameter, i.e. like this to send over MetricTelemetries:

```csharp
var log = new LoggerConfiguration()
    .WriteTo
	.ApplicationInsights("<MyApplicationInsightsInstrumentationKey>", LogEventsToMetricTelemetryConverter)
    .CreateLogger();

// ....

private static ITelemetry LogEventsToMetricTelemetryConverter(LogEvent serilogLogEvent, IFormatProvider formatProvider)
{
    var metricTelemetry = new MetricTelemetry(/* ...*/);
    // forward properties from logEvent or ignore them altogether...
    return metricTelemetry;
}

```

If you would like to return multiple `ITelemetry` items there is another overload:

```csharp
var log = new LoggerConfiguration()
    .WriteTo
	.ApplicationInsights("<MyApplicationInsightsInstrumentationKey>", LogEventsToMetricTelemetryConverter)
    .CreateLogger();

// ....

private static IEnumerable<ITelemetry> LogEventsToMetricTelemetryConverter(LogEvent serilogLogEvent, IFormatProvider formatProvider)
{
    var metricTelemetry = new MetricTelemetry(/* ...*/);
    // forward properties from logEvent or ignore them altogether...
    yield return metricTelemetry;

    var traceTelemetry = new TraceTelemetry(/* ...*/);
    // forward properties from logEvent or ignore them altogether...
    yield return traceTelemetry;

	//...
}
```

.. or alternatively by using the built-in, default TraceTelemetry generation logic, but adapt the Telemetry's Context to include a UserId, operation_Id and operation_parentId when those properties is available. By setting operation id the gui in azure will display all loggs from that operation when that item is selected:


```csharp
public static void Main()
{
    var log = new LoggerConfiguration()
        .WriteTo
        .ApplicationInsights("<MyApplicationInsightsInstrumentationKey>", ConvertLogEventsToCustomTraceTelemetry)
        .CreateLogger();
}



private static ITelemetry ConvertLogEventsToCustomTraceTelemetry(LogEvent logEvent, IFormatProvider formatProvider)
{
    // first create a default TraceTelemetry using the sink's default logic
    // .. but without the log level, and (rendered) message (template) included in the Properties
    var telemetry = GetTelematry(logEvent);

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
    return telemetry;
}

private static ITelemetry GetTelemetry(LogEvent logEvent)
{
    if (logEvent.Exception != null) {
        // Exception telemetry
        return logEvent.ToDefaultExceptionTelemetry(
        formatProvider,
        includeLogLevelAsProperty: false,
        includeRenderedMessageAsProperty: false,
        includeMessageTemplateAsProperty: false);
    }
    else {
        // default telemetry
        return logEvent.ToDefaultTraceTelemetry(
        formatProvider,
        includeLogLevelAsProperty: false,
        includeRenderedMessageAsProperty: false,
        includeMessageTemplateAsProperty: false);
    }
}

```

If you want to skip sending a particular LogEvent, just return `null` from your own converter method.


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
