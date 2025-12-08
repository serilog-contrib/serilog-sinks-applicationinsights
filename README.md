# Serilog.Sinks.ApplicationInsights [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.ApplicationInsights.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights/)

A sink for Serilog that writes events to Microsoft Application Insights. This sink comes with several defaults that send
Serilog `LogEvent` messages to Application Insights as either `EventTelemetry` or `TraceTelemetry`.

## Configuring

The simplest way to configure Serilog to send data to a Application Insights dashboard via instrumentation key is to use
current active *telemetry configuration* which is already initialised in most application types like ASP.NET Core, Azure
Functions etc.:

```csharp
// TelemetryConfiguration.Active is obsolete
// https://github.com/microsoft/ApplicationInsights-dotnet/pull/1148
var log = new LoggerConfiguration()
    .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
    .CreateLogger();
```

.. or as `EventTelemetry`:

```csharp
// TelemetryConfiguration.Active is obsolete
// https://github.com/microsoft/ApplicationInsights-dotnet/pull/1148
var log = new LoggerConfiguration()
    .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Events)
    .CreateLogger();
```

> You can also pass an *instrumentation key* and this sink will create a new `TelemetryConfiguration` based on it,
> however it's actively discouraged compared to using already initialised telemetry configuration, as your telemetry
> won't
> be properly correlated.

**Note:** Whether you choose `Events` or `Traces`, if the LogEvent contains any exceptions it will always be sent
as `ExceptionTelemetry`.

### `TelemetryConfiguration.Active` is deprecated in the App Insights SDK for .NET Core, what do I do?

The
singleton [`TelemetryConfiguration.Active` has been deprecated in the Application Insights SDK on .NET Core in favor of dependency injection pattern](https://github.com/microsoft/ApplicationInsights-dotnet/issues/1152)
.

Therefore, now we need to pass in the `TelemetryConfiguration` instance that was added either
by `services.AddApplicationInsightsTelemetryWorkerService()` (if you're developing
a [non-http application](https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service))
or `services.AddApplicationInsightsTelemetry()` (if you're developing
an [ASP.Net Core application](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)) during Startup
in `ConfigureServices`.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.ApplicationInsights(
        serviceProvider.GetRequiredService<TelemetryConfiguration>(),
	TelemetryConverter.Traces)
    .CreateLogger();
```

However, you probably want to setup your Logger as close to the entry point of your application as possible, so that any
startup errors can be caught and properly logged. The problem is that now we're in a chicken-and-egg situation: we want
to setup the logger early, but we need the `TelemetryConfiguration` which still haven't been added to our DI container.

Luckily [from version 4.0.x of the `Serilog.Extensions.Hosting` we have the possibility to configure a bootstrap logger](https://nblumhardt.com/2020/10/bootstrap-logger/)
to capture early errors, and then change it using DI dependant services once they are configured.

```csharp
// dotnet add package serilog.extensions.hosting -v 4.0.0-*

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .WriteTo.ApplicationInsights(
		    services.GetRequiredService<TelemetryConfiguration>(),
		    TelemetryConverter.Traces))
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}
```

### Configuring with `ReadFrom.Configuration()`

Configuring in code, as shown above, is recommended because the existing `TelemetryClient` can be injected.

The following configuration shows how to create an ApplicationInsights sink
with [ReadFrom.Configuration(configuration)](https://github.com/serilog/serilog-settings-configuration).

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.ApplicationInsights"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "ApplicationInsights",
        "Args": {
          "connectionString": "[your connection string here]",
          "telemetryConverter":
	    "Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights"
        }
      }
    ],
    "Enrich": [ "FromLogContext" ],
    "Properties": {
      "Application": "Sample"
    }
  }
}
```

The `telemetryConverter` has to be specified with the full type name and the assembly name.

A `connectionString` can be omitted if it's [supplied in the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable](https://docs.microsoft.com/en-us/azure/azure-monitor/app/migrate-from-instrumentation-keys-to-connection-strings).

## What does the sink submit?

By default, trace telemetry submits:

- **rendered message** in trace's standard *message* property.
- **severity** in trace's standard *severityLevel* property.
- **timestamp** in trace's standard *timestamp* property.
- **messageTemplate** in *customDimensions*.
- **custom log properties** as *customDimensions*.

Event telemetry submits:

- **message template** as *event name*.
- **renderedMessage** in *customDimensions*.
- **timestamp** in event's standard *timestamp* property.
- **custom log properties** as *customDimensions*.

Exception telemetry submits:

- **exception** as standard AI exception.
- **severity** in trace's standard *severityLevel* property.
- **timestamp** in trace's standard *timestamp* property.
- **custom log properties** as *customDimensions*.

> Note that **log context** properties are also included in *customDimensions* when Serilog is configured
> with `.Enrich.FromLogContext()`.

## How custom properties are logged?

By default custom properties are converted to compact JSON, for instance:

```csharp
var position = new { Latitude = 25, Longitude = 134 };
var elapsedMs = 34;
var numbers = new int[] { 1, 2, 3, 4 };

Logger.Information("Processed {@Position} in {Elapsed:000} ms., str {str}, numbers: {numbers}",
    position, elapsedMs, "test", numbers);
```

will produce the following properties in *customDimensions*:

|Property|Value|
|--------|-----|
|Elapsed|34|
|Position|{"Latitude":25,"Longitude":134}|
|numbers|[1,2,3,4]|

This is a breaking change from v2 which was producing these properties:

|Property|Value|
|--------|-----|
|Elapsed|34|
|Position.Latitude|25|
|Position.Longitude|134|
|numbers.0|1|
|numbers.1|2|
|numbers.2|3|
|numbers.3|4|

You can revert the old behavior by overriding standard telemetry formatter, for instance:

```csharp
private class DottedOutTraceTelemetryConverter : TraceTelemetryConverter
{
    public override IValueFormatter ValueFormatter => new ApplicationInsightsDottedValueFormatter();
}
```

## Customizing

Additionally, you can also customize *whether* to send the LogEvents at all, if so *which type(s)* of Telemetry to send
and also *what to send* (all or no LogEvent properties at all) by passing your own `ITelemetryConverter` instead
of `TelemetryConverter.Traces` or `TelemetryConverter.Events` by either implementing your own `ITelemetryConverter` or
deriving from `TraceTelemetryConverter` or `EventTelemetryConverter` and overriding specific bits.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.ApplicationInsights(configuration, new CustomConverter())
    .CreateLogger();
// ...

private class CustomConverter : TraceTelemetryConverter
{
    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        // first create a default TraceTelemetry using the sink's default logic
        // .. but without the log level, and (rendered) message (template) included in the Properties
        foreach (ITelemetry telemetry in base.Convert(logEvent, formatProvider))
        {
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

            // find redundant properties
            var removeProps = new[] { "UserId", "operation_parentId", "operation_Id" };
            removeProps = removeProps.Where(prop => propTelematry.Properties.ContainsKey(prop)).ToArray();

            foreach (var prop in removeProps)
            {
                // remove redundant properties
                propTelematry.Properties.Remove(prop);
            }

            yield return telemetry;
        }
    }
}
```

If you want to skip sending a particular LogEvent, just return `null` from your own converter method.

### Customising included properties

The easiest way to customise included properties is to subclass one of the `ITelemetryConverter` implementations. For
instance, let's include `renderedMessage` in event telemetry:

```csharp
private class IncludeRenderedMessageConverter : EventTelemetryConverter
{
    public override void ForwardPropertiesToTelemetryProperties(LogEvent logEvent, 
        ISupportProperties telemetryProperties, IFormatProvider formatProvider)
    {
        base.ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
            includeLogLevel: false,
            includeRenderedMessage: true,
            includeMessageTemplate: false);
    }
}
```

## How, When and Why to Flush Messages Manually

### Or: Where did my Messages go?

As explained by
the [Application Insights documentation](https://azure.microsoft.com/en-us/documentation/articles/app-insights-api-custom-events-metrics/#flushing-data)
, the default behaviour of the AI client is to buffer messages and send them to AI in batches whenever the client seems
fit. However, this may lead to lost messages when your application terminates while there are still unsent messages in
said buffer.

You can control when AI shall flush its messages, for example when your application closes:

1) Create a custom `TelemetryClient` and hold on to it in a field or property:

```csharp
// private TelemetryClient _telemetryClient;

// ...
_telemetryClient = new TelemetryClient()
            {
                InstrumentationKey = "<My AI Instrumentation Key>"
            };
```

2) Use that custom `TelemetryClient` to initialize the Sink:

```csharp
var log = new LoggerConfiguration()
    .WriteTo
	.ApplicationInsights(_telemetryClient, TelemetryConverter.Events)
    .CreateLogger();
```

3) Call .Flush() on the TelemetryClient whenever you deem necessary, i.e. Application Shutdown:

```csharp
_telemetryClient.Flush();

// The AI documentation mentions that calling `Flush()` *can* be asynchronous and non-blocking so
// depending on the underlying Channel to AI you might want to wait some time
// specific to your application and its connectivity constraints for the flush to finish.

await Task.Delay(1000);

// or 

System.Threading.Thread.Sleep(1000);

```

## Including Operation Id

Application Insight's operation id is pushed out if you set `operationId` LogEvent property. If it's present, AI's
operation id will be overridden by the value from this property.

This can be set like so:

```csharp

public class OperationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("RequestId", out var requestId))
        {
            logEvent.AddPropertyIfAbsent(new LogEventProperty("operationId", requestId));
        }
    }
}
```

## Including Version

Application Insight supports component version and is pushed out if you set `version` log event property. If it's
present, AI's operation version will include the value from this property.

## Using with Azure Functions

Azure functions has out of the box integration with Application Insights, which automatically logs functions execution
start, end, and any exception. Please refer to
the [original documenation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring) on how to
enable it.

This sink can enrich AI messages, preserving *operation_Id* and other context information which is *already provided by
functions runtime*. The easiest way to configure Serilog in this case is to use the injected **TelemetryClient** which
should be automatically configured by the environment through the **APPLICATIONINSIGHTS_CONNECTION_STRING** appsetting.
You can, for instance, initialise logging in the static constructor:

```csharp
[assembly: FunctionsStartup(typeof(MyFunctions.Startup))]
namespace MyFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider>((sp) => 
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.ApplicationInsights(sp.GetRequiredService<TelemetryClient>(), TelemetryConverter.Traces)
                    .CreateLogger();
                return new SerilogLoggerProvider(Log.Logger, true);
            });
        }
    }
}
```

Copyright &copy; 2022 Serilog Contributors - Provided under
the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html).

See also: [Serilog Documentation](https://github.com/serilog/serilog/wiki)
