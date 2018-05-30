using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace AspNetCoreSample.Filters
{
    /// <summary>
    /// Logs alls <see cref="System.Exception"/>s and enriches their telemetry data.
    /// </summary>
    /// <seealso cref="EventTelemetry"/>
    /// <seealso cref="TelemetryClient"/>
    public class LoggingExceptionFilter: IExceptionFilter
    {
        private readonly ILogger<LoggingExceptionFilter> _logger;

        public LoggingExceptionFilter(ILogger<LoggingExceptionFilter> logger)
        {
            _logger = logger;
            _logger.LogTrace("Initialized {Type}", this.GetType().AssemblyQualifiedName);
        }

        public void OnException(ExceptionContext context)
        {
            using (_logger.BeginScope(new {Url = context.HttpContext.Request.GetDisplayUrl() }))
            {
                var eventTelemetry = context.HttpContext.Features.Get<EventTelemetry>();
                eventTelemetry.Name = "UnhandledException";
                eventTelemetry.Context.Properties["ExceptionType"] = context.Exception.GetType().AssemblyQualifiedName;
                _logger.LogError(context.Exception, "An unhandled exception occurred");
                context.HttpContext.Features.Get<TelemetryClient>().TrackEvent(eventTelemetry);
            }
        }
    }
}
