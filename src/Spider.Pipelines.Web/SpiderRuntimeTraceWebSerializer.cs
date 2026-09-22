namespace Spider.Pipelines.Web
{
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using Spider.Pipelines.RuntimeTracing;

    /// <summary>
    /// Serializes runtime trace data for the Spider web UI.
    /// </summary>
    public sealed class SpiderRuntimeTraceWebSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.Default
        };

        /// <summary>
        /// Serializes runtime traces and summaries.
        /// </summary>
        /// <param name="summaries">The runtime trace summaries.</param>
        /// <param name="traces">The runtime traces.</param>
        /// <returns>The serialized runtime trace payload.</returns>
        public string Serialize(
            IReadOnlyCollection<SpiderTraceSummary> summaries,
            IReadOnlyCollection<SpiderTrace> traces)
        {
            var payload = CreatePayload(summaries, traces);
            return JsonSerializer.Serialize(payload, Options).Replace("</", "<\\/", StringComparison.Ordinal);
        }

        /// <summary>
        /// Creates a JSON-ready runtime trace payload.
        /// </summary>
        /// <param name="summaries">The runtime trace summaries.</param>
        /// <param name="traces">The runtime traces.</param>
        /// <returns>The runtime trace payload.</returns>
        public static object CreatePayload(
            IReadOnlyCollection<SpiderTraceSummary> summaries,
            IReadOnlyCollection<SpiderTrace> traces)
            => new
            {
                summaries = (summaries ?? Array.Empty<SpiderTraceSummary>()).Select(summary => new
                {
                    traceId = summary.TraceId,
                    rootComponentId = summary.RootComponentId,
                    rootDisplayName = summary.RootDisplayName,
                    requestType = summary.RequestType,
                    responseType = summary.ResponseType,
                    status = summary.Status.ToString(),
                    startedAt = summary.StartedAt,
                    completedAt = summary.CompletedAt,
                    durationMs = summary.Duration?.TotalMilliseconds,
                    eventCount = summary.EventCount,
                    droppedEventCount = summary.DroppedEventCount
                }).ToArray(),
                traces = (traces ?? Array.Empty<SpiderTrace>()).Select(trace => new
                {
                    traceId = trace.TraceId,
                    correlationId = trace.CorrelationId,
                    status = trace.Status.ToString(),
                    startedAt = trace.StartedAt,
                    completedAt = trace.CompletedAt,
                    durationMs = trace.Duration?.TotalMilliseconds,
                    events = trace.Events.Select(traceEvent => new
                    {
                        traceId = traceEvent.TraceId,
                        spanId = traceEvent.SpanId,
                        parentSpanId = traceEvent.ParentSpanId,
                        componentId = traceEvent.ComponentId,
                        componentKind = traceEvent.ComponentKind,
                        displayName = traceEvent.DisplayName,
                        operation = traceEvent.Operation,
                        kind = traceEvent.Kind.ToString(),
                        status = traceEvent.Status.ToString(),
                        timestamp = traceEvent.Timestamp,
                        durationMs = traceEvent.Duration?.TotalMilliseconds,
                        inputType = traceEvent.InputType,
                        outputType = traceEvent.OutputType,
                        exception = traceEvent.Exception == null ? null : new
                        {
                            type = traceEvent.Exception.Type,
                            message = traceEvent.Exception.Message
                        },
                        tags = traceEvent.Tags,
                        metadata = traceEvent.Metadata
                    }).ToArray()
                }).ToArray()
            };
    }
}
