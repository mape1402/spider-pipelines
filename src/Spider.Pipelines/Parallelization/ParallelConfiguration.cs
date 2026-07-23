namespace Spider.Pipelines.Parallelization
{
    using System.Collections.Immutable;

    /// <summary>
    /// Provides configuration for parallel processing steps and builds their execution logic for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class ParallelConfiguration<TRequest> : IParallelConfiguration<TRequest>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IList<ParallelProcessDelegate<TRequest>> _parallelDelegates;
        private ParallelExecutionMode _mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelConfiguration{TRequest}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public ParallelConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _parallelDelegates = new List<ParallelProcessDelegate<TRequest>>();
            _mode = ParallelExecutionMode.WithTarget;
        }

        /// <inheritdoc/>
        public IParallelConfiguration<TRequest> OnParallel(ParallelProcessDelegate<TRequest> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _parallelDelegates.Add(handler);
            return this;
        }

        /// <inheritdoc/>
        public IParallelConfiguration<TRequest> WithMode(ParallelExecutionMode mode)
        {
            _mode = mode;
            return this;
        }

        /// <inheritdoc/>
        public IParallelExecution<TRequest> BuildExecution()
            => new ParallelExecution<TRequest>(_parallelDelegates.ToImmutableArray(), _mode);
    }

    /// <summary>
    /// Provides configuration for parallel processing steps and builds their execution logic for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class ParallelConfiguration<TRequest, TResponse> : IParallelConfiguration<TRequest, TResponse>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IList<ParallelProcessDelegate<TRequest>> _parallelDelegates;
        private ParallelExecutionMode _mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelConfiguration{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public ParallelConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _parallelDelegates = new List<ParallelProcessDelegate<TRequest>>();
            _mode = ParallelExecutionMode.WithTarget;
        }

        /// <inheritdoc/>
        public IParallelConfiguration<TRequest, TResponse> OnParallel(ParallelProcessDelegate<TRequest> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _parallelDelegates.Add(handler);
            return this;
        }

        /// <inheritdoc/>
        public IParallelConfiguration<TRequest, TResponse> WithMode(ParallelExecutionMode mode)
        {
            _mode = mode;
            return this;
        }

        /// <inheritdoc/>
        public IParallelExecution<TRequest, TResponse> BuildExecution()
            => new ParallelExecution<TRequest, TResponse>(_parallelDelegates.ToImmutableArray(), _mode);
    }
}
