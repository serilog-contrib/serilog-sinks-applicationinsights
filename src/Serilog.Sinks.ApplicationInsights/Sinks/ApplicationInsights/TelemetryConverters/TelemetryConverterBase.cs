using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.ApplicationInsights.Formatters;

namespace Serilog.Sinks.ApplicationInsights.TelemetryConverters;

/// <summary>
///     Base class for telemetry converters
/// </summary>
public abstract class TelemetryConverterBase : ITelemetryConverter
{
    /// <summary>
    /// The <see cref="LogEvent.Level" />
    /// is forwarded to the underlying AI Telemetry and its .Properties using this key.
    /// </summary>
    public const string TelemetryPropertiesLogLevel = "LogLevel";

    /// <summary>
    ///     The <see cref="LogEvent.MessageTemplate" /> is forwarded to the underlying AI Telemetry and its .Properties using
    ///     this key.
    /// </summary>
    public const string TelemetryPropertiesMessageTemplate = "MessageTemplate";

    /// <summary>
    ///     The result of <see cref="LogEvent.RenderMessage(System.IFormatProvider)" /> is forwarded to the underlying AI
    ///     Telemetry and its .Properties using this key.
    /// </summary>
    public const string TelemetryPropertiesRenderedMessage = "RenderedMessage";

    /// <summary>
    ///     Property that is included when in log context, will be pushed out as AI operation ID.
    /// </summary>
    public const string OperationIdProperty = "operationId";

    /// <summary>
    ///     Property that is included when in log context, will be pushed out as AI parent span id.
    /// </summary>
    public const string ParentSpanIdProperty = "ParentSpanId";

    /// <summary>
    ///     Property that is included when in log context, will be pushed out as AI operation name.
    /// </summary>
    public const string OperationNameProperty = "OperationName";

    /// <summary>
    ///     Property that is included when in log context, will be pushed out as AI telemetry properties.
    /// </summary>
    public const string BaggageProperty = "Baggage";

    /// <summary>
    ///     Property that is included when in log context, will be pushed out as AI component version.
    /// </summary>
    public const string VersionProperty = "version";

    static readonly MessageTemplateTextFormatter MessageTemplateTextFormatter = new("{Message:lj}");

    readonly bool _includeOperationIdPropertyAsTelemetryProperty;
    readonly bool _includeParentSpanIdPropertyAsTelemetryProperty;
    readonly bool _includeOperationNamePropertyAsTelemetryProperty;
    readonly bool _includeVersionPropertyAsTelemetryProperty;

    /// <summary>
    ///     Creates an instance of <see cref="TelemetryConverterBase" /> using default value formatter (
    ///     <see cref="ApplicationInsightsJsonValueFormatter" />).
    /// </summary>
    public TelemetryConverterBase()
        : this(false, false, false, false)
    {
    }

    /// <summary>
    ///     Creates an instance of <see cref="TelemetryConverterBase" /> using default value formatter (
    ///     <see cref="ApplicationInsightsJsonValueFormatter" />).
    /// </summary>
    /// <param name="includeOperationIdPropertyAsTelemetryProperty">
    ///   if set to <c>true</c> the <see cref="OperationIdProperty" /> is added to the
    ///   telemetry properties. Otherwise it is only set as <c>ITelemetry.Context.Operation.Id</c>.
    /// </param>
    /// <param name="includeParentSpanIdPropertyAsTelemetryProperty">
    ///   if set to <c>true</c> the <see cref="ParentSpanIdProperty" /> is added to the
    ///   telemetry properties. Otherwise it is only set as <c>ITelemetry.Context.Operation.ParentId</c>.
    /// </param>
    /// <param name="includeOperationNamePropertyAsTelemetryProperty">
    ///   if set to <c>true</c> the <see cref="OperationNameProperty" /> is added to the
    ///   telemetry properties. Otherwise it is only set as <c>ITelemetry.Context.Operation.Name</c>.
    /// </param>
    /// <param name="includeVersionPropertyAsTelemetryProperty">
    ///   if set to <c>true</c> the <see cref="VersionProperty" /> is added to the
    ///   telemetry properties. Otherwise it is only set as <c>ITelemetry.Context.Component.Version</c>.
    /// </param>
    public TelemetryConverterBase(
        bool includeOperationIdPropertyAsTelemetryProperty,
        bool includeParentSpanIdPropertyAsTelemetryProperty,
        bool includeOperationNamePropertyAsTelemetryProperty,
        bool includeVersionPropertyAsTelemetryProperty)
    {
        ValueFormatter = new ApplicationInsightsJsonValueFormatter();

        _includeOperationIdPropertyAsTelemetryProperty = includeOperationIdPropertyAsTelemetryProperty;
        _includeParentSpanIdPropertyAsTelemetryProperty = includeParentSpanIdPropertyAsTelemetryProperty;
        _includeOperationNamePropertyAsTelemetryProperty = includeOperationNamePropertyAsTelemetryProperty;
        _includeVersionPropertyAsTelemetryProperty = includeVersionPropertyAsTelemetryProperty;
    }

#pragma warning disable CS1591
    public virtual IValueFormatter ValueFormatter { get; }
#pragma warning restore CS1591

#pragma warning disable CS1591
    public abstract IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider);
#pragma warning restore CS1591

#pragma warning disable CS1591
    public virtual ExceptionTelemetry ToExceptionTelemetry(
#pragma warning restore CS1591
        LogEvent logEvent,
        IFormatProvider formatProvider)
    {
        if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
        if (logEvent.Exception == null) throw new ArgumentException("Must have an Exception", nameof(logEvent));

        var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception)
        {
            SeverityLevel = ToSeverityLevel(logEvent.Level),
            Timestamp = logEvent.Timestamp
        };

        // write logEvent's .Properties to the AI one
        ForwardPropertiesToTelemetryProperties(logEvent, exceptionTelemetry, formatProvider);

        return exceptionTelemetry;
    }

#pragma warning disable CS1591
    public virtual void ForwardPropertiesToTelemetryProperties(LogEvent logEvent,
#pragma warning restore CS1591
        ISupportProperties telemetryProperties,
        IFormatProvider formatProvider)
    {
        ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
            includeLogLevel: false,
            includeRenderedMessage: false,
            includeMessageTemplate: true);
    }

    /// <summary>
    ///     Forwards all <see cref="LogEvent" /> data to the <paramref name="telemetryProperties" /> including the log level,
    ///     rendered message, message template and all <paramref name="logEvent" /> properties to the telemetry.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="telemetryProperties">The telemetry properties.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="includeLogLevel">
    ///     if set to <c>true</c> the <see cref="LogEvent.Level" /> is added to the
    ///     <paramref name="telemetryProperties" /> using the <see cref="TelemetryPropertiesLogLevel" /> key.
    /// </param>
    /// <param name="includeRenderedMessage">
    ///     if set to <c>true</c> the <see cref="LogEvent.RenderMessage(System.IFormatProvider)" /> output is added to the
    ///     <paramref name="telemetryProperties" /> using the <see cref="TelemetryPropertiesRenderedMessage" /> key.
    /// </param>
    /// <param name="includeMessageTemplate">
    ///     if set to <c>true</c> the <see cref="LogEvent.MessageTemplate" /> is added to the
    ///     <paramref name="telemetryProperties" /> using the <see cref="TelemetryPropertiesMessageTemplate" /> key.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="logEvent" /> or
    ///     <paramref name="telemetryProperties" /> is null.
    /// </exception>
    public void ForwardPropertiesToTelemetryProperties(LogEvent logEvent,
        ISupportProperties telemetryProperties,
        IFormatProvider formatProvider,
        bool includeLogLevel,
        bool includeRenderedMessage,
        bool includeMessageTemplate)
    {
        if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
        if (telemetryProperties == null) throw new ArgumentNullException(nameof(telemetryProperties));

        if (includeLogLevel)
            telemetryProperties.Properties.Add(TelemetryPropertiesLogLevel, Enum.GetName(typeof(LogEventLevel), logEvent.Level));

        if (includeRenderedMessage)
        {
            var sw = new StringWriter();
            MessageTemplateTextFormatter.Format(logEvent, sw);
            telemetryProperties.Properties.Add(TelemetryPropertiesRenderedMessage, sw.ToString());
        }

        if (includeMessageTemplate)
            telemetryProperties.Properties.Add(TelemetryPropertiesMessageTemplate, logEvent.MessageTemplate.Text);

        if (telemetryProperties is ITelemetry telemetry)
        {
            PopulateTelemetryFromLogEvent(logEvent, telemetry);

            if (telemetry.Context?.Component != null
                && logEvent.Properties.TryGetValue(VersionProperty, out var version))
                telemetry.Context.Component.Version = version.ToString().Trim('\"');
        }

        var baggageWasForwarded = ForwardActivityBaggage(logEvent, telemetryProperties, formatProvider);
        ForwardSimpleProperties(logEvent, telemetryProperties, baggageWasForwarded);
    }

    private static void PopulateTelemetryFromLogEvent(LogEvent logEvent, ITelemetry telemetry)
    {
        // Operation.Id (TraceId)
        if (logEvent.Properties.TryGetValue(OperationIdProperty, out var operationIdProp))
            telemetry.Context.Operation.Id = operationIdProp.ToString().Trim('"');
        else if (logEvent.TraceId is ActivityTraceId traceId)
            telemetry.Context.Operation.Id = traceId.ToHexString();

        // Operation.ParentId (ParentSpanId)
        if (logEvent.Properties.TryGetValue(ParentSpanIdProperty, out var parentSpanIdProp))
            telemetry.Context.Operation.ParentId = parentSpanIdProp.ToString().Trim('"');

        // Operation.Name (OperationName)
        if (logEvent.Properties.TryGetValue(OperationNameProperty, out var operationNameProp))
            telemetry.Context.Operation.Name = operationNameProp.ToString().Trim('"');

        // Set Id for RequestTelemetry and DependencyTelemetry
        if (logEvent.SpanId is ActivitySpanId spanId)
        {
            if (telemetry is RequestTelemetry req)
                req.Id = spanId.ToHexString();
            else if (telemetry is DependencyTelemetry dep)
                dep.Id = spanId.ToHexString();
        }
    }

    private static bool ForwardActivityBaggage(LogEvent logEvent, ISupportProperties telemetryProperties, IFormatProvider formatProvider)
    {
        if (!logEvent.Properties.TryGetValue(BaggageProperty, out var baggageProp)
            || baggageProp is not StructureValue baggageStructure)
        {
            return false;
        }

        foreach (var item in baggageStructure.Properties)
        {
            var key = item.Name;
            if (telemetryProperties.Properties.ContainsKey(key))
            {
                continue;
            }

            var value = item.Value.ToString(null, formatProvider).Trim('"');
            telemetryProperties.Properties.Add(key, value);
        }

        return true;
    }

    private void ForwardSimpleProperties(LogEvent logEvent, ISupportProperties telemetryProperties, bool skipBaggage)
    {
        var skipOperationId = !_includeOperationIdPropertyAsTelemetryProperty;
        var skipParentSpanId = !_includeParentSpanIdPropertyAsTelemetryProperty;
        var skipOperationName = !_includeOperationNamePropertyAsTelemetryProperty;
        var skipVersion = !_includeVersionPropertyAsTelemetryProperty;

        foreach (var property in logEvent.Properties)
        {
            if (property.Value is null) continue;
            if (skipOperationId && OperationIdProperty.Equals(property.Key, StringComparison.Ordinal)) continue;
            if (skipParentSpanId && ParentSpanIdProperty.Equals(property.Key, StringComparison.Ordinal)) continue;
            if (skipOperationName && OperationNameProperty.Equals(property.Key, StringComparison.Ordinal)) continue;
            if (skipVersion && VersionProperty.Equals(property.Key, StringComparison.Ordinal)) continue;
            if (skipBaggage && BaggageProperty.Equals(property.Key, StringComparison.Ordinal)) continue;
            if (telemetryProperties.Properties.ContainsKey(property.Key)) continue;

            ValueFormatter.Format(property.Key, property.Value, telemetryProperties.Properties);
        }
    }

    /// <summary>
    ///     To the severity level.
    /// </summary>
    /// <param name="logEventLevel">The log event level.</param>
    /// <returns></returns>
    public SeverityLevel? ToSeverityLevel(LogEventLevel logEventLevel)
    {
        switch (logEventLevel)
        {
            case LogEventLevel.Verbose:
            case LogEventLevel.Debug:
                return SeverityLevel.Verbose;
            case LogEventLevel.Information:
                return SeverityLevel.Information;
            case LogEventLevel.Warning:
                return SeverityLevel.Warning;
            case LogEventLevel.Error:
                return SeverityLevel.Error;
            case LogEventLevel.Fatal:
                return SeverityLevel.Critical;
        }

        return null;
    }
}
