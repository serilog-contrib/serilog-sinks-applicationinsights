# Serilog.Sinks.ApplicationInsights

A sink for Serilog that writes events to Microsoft Application Insights.
 
[![Build status](https://ci.appveyor.com/api/projects/status/ccgd7k98kbmifl5v/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-applicationinsights/branch/master) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.ApplicationInsights.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights/)

Writes log events to Application Insights as `EventTelemetry`:

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

* [Documentation](https://github.com/serilog/serilog/wiki)

Copyright &copy; 2016 Serilog Contributors - Provided under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html).