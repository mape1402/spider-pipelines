namespace Spider.Pipelines.PostProcessing
{
    using System.Collections.Immutable;

    /// <summary>
    /// Provides configuration for post-processing steps and builds their execution logic for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public sealed class PostProcessConfiguration<TRequest> : IPostProcessConfiguration<TRequest>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IList<FailurePostProcessDelegate<TRequest>> _failureDelegates;
        private readonly IList<SuccessPostProcessDelegate<TRequest>> _successDelagates;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessConfiguration{TRequest}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public PostProcessConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _failureDelegates = new List<FailurePostProcessDelegate<TRequest>>();
            _successDelagates = new List<SuccessPostProcessDelegate<TRequest>>();
        }

        /// <inheritdoc/>
        public IPostProcessConfiguration<TRequest> OnFailure(FailurePostProcessDelegate<TRequest> handler)
        {
            _failureDelegates.Add(handler);
            return this;
        }

        /// <inheritdoc/>
        public IPostProcessConfiguration<TRequest> OnSuccess(SuccessPostProcessDelegate<TRequest> handler)
        {
            _successDelagates.Add(handler);
            return this;
        }

        /// <inheritdoc/>
        public IPostProcessExecution<TRequest> BuildExecution()
            => new PostProcessExecution<TRequest>(_successDelagates.ToImmutableArray(), _failureDelegates.ToImmutableArray());
    }

    /// <summary>
    /// Provides configuration for post-processing steps and builds their execution logic for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public sealed class PostProcessConfiguration<TRequest, TResponse> : IPostProcessConfiguration<TRequest, TResponse>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IList<FailurePostProcessDelegate<TRequest>> _failureDelegates;
        private readonly IList<SuccessPostProcessDelegate<TRequest, TResponse>> _successDelagates;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessConfiguration{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public PostProcessConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _failureDelegates = new List<FailurePostProcessDelegate<TRequest>>();
            _successDelagates = new List<SuccessPostProcessDelegate<TRequest, TResponse>>();
        }

        /// <inheritdoc/>
        public IPostProcessConfiguration<TRequest, TResponse> OnFailure(FailurePostProcessDelegate<TRequest> handler)
        {
            _failureDelegates.Add(handler);
            return this;
        }

        /// <inheritdoc/>
        public IPostProcessConfiguration<TRequest, TResponse> OnSuccess(SuccessPostProcessDelegate<TRequest, TResponse> handler)
        {
            _successDelagates.Add(handler);
            return this;
        }

        /// <inheritdoc/>
        public IPostProcessExecution<TRequest, TResponse> BuildExecution()
            => new PostProcessExecution<TRequest, TResponse>(_successDelagates.ToImmutableArray(), _failureDelegates.ToImmutableArray());
    }
}
