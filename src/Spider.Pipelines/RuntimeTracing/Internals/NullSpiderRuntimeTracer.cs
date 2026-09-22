namespace Spider.Pipelines.RuntimeTracing.Internals
{
    /// <summary>
    /// Provides a no-op runtime tracer used when tracing is disabled.
    /// </summary>
    internal sealed class NullSpiderRuntimeTracer : ISpiderRuntimeTracer
    {
        /// <summary>
        /// Gets the singleton no-op tracer instance.
        /// </summary>
        public static readonly NullSpiderRuntimeTracer Instance = new();

        private NullSpiderRuntimeTracer()
        {
        }

        /// <inheritdoc/>
        public bool IsEnabled => false;

        /// <inheritdoc/>
        public ValueTask<SpiderTraceScope> StartSpanAsync(
            SpiderTraceSpanDefinition definition,
            CancellationToken cancellationToken = default)
            => new((SpiderTraceScope)null);

        /// <inheritdoc/>
        public ValueTask AddEventAsync(
            SpiderTraceEvent traceEvent,
            CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
