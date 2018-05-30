using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace AspNetCoreSample.Filters
{
    public class RequestTelemetryEnricher : ResultFilterAttribute
    {
        private readonly ILogger<RequestTelemetryEnricher> _logger;
        private readonly IOptions<RequestTelemetryEnricherOptions> _options;

        public RequestTelemetryEnricher(ILogger<RequestTelemetryEnricher> logger, IOptions<RequestTelemetryEnricherOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            _logger.LogTrace("{ClassName:l}.OnResultExecuting.", this.GetType().FullName);
            var controllerFqn = context.Controller.GetType().AssemblyQualifiedName;
            LogContext.PushProperty("ControllerFullyQualifiedName", controllerFqn);
            var requestTelemetry = context.HttpContext.Features.Get<RequestTelemetry>();
            requestTelemetry.Context.Properties["ControllerFullyQualifiedName"] = controllerFqn;
            if (_options.Value.LogRequestBody)
            {
                if (context.HttpContext.Request.Body == null)
                {
                    _logger.LogTrace("Requestof type {Method} has no body.", context.HttpContext.Request.Method);
                }
                //TODO: This is kind of a pain to do right and rewind the body.
                _logger.LogWarning("Skipping Logging Request Body");
            }
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            _logger.LogTrace("{ClassName:l}.OnResultExecuted.", this.GetType().FullName);
            if (_options.Value.LogResponseBody)
            {
                //TODO: This is kind of a pain to do right and rewind the body.
                _logger.LogWarning("Skipping Logging Respone Body");
                }
        }
    }
}