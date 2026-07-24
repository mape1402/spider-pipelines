namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Provides provider-agnostic metadata for a typed request-only pipeline execution boundary.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public class PipelineExecutionContext<TRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineExecutionContext{TRequest}"/> class.
        /// </summary>
        /// <param name="request">The request instance being processed.</param>
        /// <param name="services">The service provider associated with the current execution.</param>
        public PipelineExecutionContext(TRequest request, IServiceProvider services)
        {
            Request = request;
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets the request instance being processed.
        /// </summary>
        public TRequest Request { get; }

        /// <summary>
        /// Gets the service provider associated with the current execution.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Gets arbitrary metadata shared by boundary callbacks during this execution.
        /// </summary>
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
    }
}
