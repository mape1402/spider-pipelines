namespace Spider.Pipelines.Parallelization
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Executes a collection of parallel processing delegates for a given request type in the pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal class ParallelExecution<TRequest> : IParallelExecution<TRequest>
    {
        /// <summary>
        /// The collection of delegates to execute in parallel.
        /// </summary>
        private readonly IReadOnlyCollection<ParallelProcessDelegate<TRequest>> _parallelProcesses;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelExecution{TRequest}"/> class.
        /// </summary>
        /// <param name="parallelProcess">The collection of parallel processing delegates to execute.</param>
        public ParallelExecution(IReadOnlyCollection<ParallelProcessDelegate<TRequest>> parallelProcess)
        {
            _parallelProcesses = parallelProcess ?? Array.Empty<ParallelProcessDelegate<TRequest>>();
        }

        /// <inheritdoc/>
        public async Task OnParallelAsync(IReadOnlyContext<TRequest> context)
        {
            if (!_parallelProcesses.Any())
                return;

            //TODO: Get standard configuration
            var arguments = new ParallelProcessArguments();

            await ParallelExecutionExtensions.ExecuteParallelInternalAsync(
                context,
                _parallelProcesses,
                arguments,
                context.CancellationToken);
        }
    }

    /// <summary>
    /// Executes a collection of parallel processing delegates for a given request and response type in the pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal class ParallelExecution<TRequest, TResponse> : IParallelExecution<TRequest, TResponse>
    {
        /// <summary>
        /// The collection of delegates to execute in parallel.
        /// </summary>
        private readonly IReadOnlyCollection<ParallelProcessDelegate<TRequest>> _parallelProcesses;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelExecution{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="parallelProcesses">The collection of parallel processing delegates to execute.</param>
        public ParallelExecution(IReadOnlyCollection<ParallelProcessDelegate<TRequest>> parallelProcesses)
        {
            _parallelProcesses = parallelProcesses ?? Array.Empty<ParallelProcessDelegate<TRequest>>();
        }

        /// <inheritdoc/>
        public async Task OnParallelAsync(IReadOnlyContext<TRequest, TResponse> context)
        {
            if (!_parallelProcesses.Any())
                return;

            //TODO: Get standard configuration
            var arguments = new ParallelProcessArguments();

            await ParallelExecutionExtensions.ExecuteParallelInternalAsync(
                context,
                _parallelProcesses,
                arguments,
                context.CancellationToken);
        }
    }
}
