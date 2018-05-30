namespace AspNetCoreSample.Filters
{
    public sealed class RequestTelemetryEnricherOptions
    {
        public bool LogRequestBody { get; set; }
        public bool LogResponseBody { get; set; }
    }
}