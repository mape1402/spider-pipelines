namespace Spider.Pipelines.PreProcessing
{
    using System.Collections.Immutable;

    /// <summary>
    /// Provides configuration for preprocessing steps and builds their execution logic for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class PreProcessConfiguration<TRequest> : IPreProcessConfiguration<TRequest>
    {
        /// <summary>
        /// The service provider for dependency resolution.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// The list of preprocessing delegates to execute.
        /// </summary>
        private readonly IList<PreProcessDelegate<TRequest>> _preprocessDelegates;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreProcessConfiguration{TRequest}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public PreProcessConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _preprocessDelegates = new List<PreProcessDelegate<TRequest>>();
        }

        /// <inheritdoc/>
        public IPreProcessConfiguration<TRequest> OnPreProcess(PreProcessDelegate<TRequest> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _preprocessDelegates.Add(handler);
            return this;
        }

        /// <inheritdoc/>
        public IPreProcessExecution<TRequest> BuildExecution()
            => new PreProcessExecution<TRequest>(_preprocessDelegates.ToImmutableArray());
    }
}
