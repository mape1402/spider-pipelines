namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Stores the current runtime trace context with async-local semantics.
    /// </summary>
    public sealed class SpiderTraceContextAccessor : ISpiderTraceContextAccessor
    {
        private readonly AsyncLocal<SpiderTraceContext> _current = new();

        /// <inheritdoc/>
        public SpiderTraceContext Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }
    }
}
