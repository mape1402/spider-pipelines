namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Provides provider-agnostic metadata for a typed request/response pipeline execution boundary.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public sealed class PipelineExecutionContext<TRequest, TResponse> : PipelineExecutionContext<TRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineExecutionContext{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="request">The request instance being processed.</param>
        /// <param name="services">The service provider associated with the current execution.</param>
        public PipelineExecutionContext(TRequest request, IServiceProvider services) : base(request, services) { }

        /// <summary>
        /// Gets the response instance produced by the pipeline.
        /// </summary>
        public TResponse Response { get; private set; }

        /// <summary>
        /// Sets the response produced by the pipeline.
        /// </summary>
        /// <param name="response">The response produced by the pipeline.</param>
        internal void SetResponse(TResponse response)
        {
            Response = response;
        }
    }
}
