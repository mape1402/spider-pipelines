namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Provides Spider-owned metadata for a pipeline execution boundary.
    /// </summary>
    public sealed class PipelineExecutionContext
    {
        /// <summary>
        /// Gets or initializes the type of the request being processed.
        /// </summary>
        public Type RequestType { get; init; }

        /// <summary>
        /// Gets or initializes the type of the response being produced, or <c>null</c> for request-only pipelines.
        /// </summary>
        public Type ResponseType { get; init; }

        /// <summary>
        /// Gets or initializes the request instance being processed.
        /// </summary>
        public object Request { get; init; }

        /// <summary>
        /// Gets or initializes the service provider associated with the current pipeline execution.
        /// </summary>
        public IServiceProvider Services { get; init; }

        /// <summary>
        /// Gets arbitrary metadata shared by boundary implementations during this execution.
        /// </summary>
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
    }
}
