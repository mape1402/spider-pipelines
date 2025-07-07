namespace Spider.Pipelines.PostProcessing
{
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;

    /// <summary>
    /// Executes post-processing delegates after the main pipeline operation for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class PostProcessExecution<TRequest> : IPostProcessExecution<TRequest>
    {
        private readonly IReadOnlyCollection<SuccessPostProcessDelegate<TRequest>> _successDelagates;
        private readonly IReadOnlyCollection<FailurePostProcessDelegate<TRequest>> _failureDelegates;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessExecution{TRequest}"/> class.
        /// </summary>
        /// <param name="successDelagates">The collection of delegates to execute after a successful operation.</param>
        /// <param name="failureDelegates">The collection of delegates to execute after a failed operation.</param>
        public PostProcessExecution(IReadOnlyCollection<SuccessPostProcessDelegate<TRequest>> successDelagates, IReadOnlyCollection<FailurePostProcessDelegate<TRequest>> failureDelegates)
        {
            _successDelagates = successDelagates;
            _failureDelegates = failureDelegates;
        }

        /// <inheritdoc/>
        public async Task OnFailureAsync(IReadOnlyContext<TRequest> context)
        {
            if (context.IsCancelled())
                return;

            //TODO: Get standard configuration
            var arguments = new PostProcessArguments();

            foreach (var process in _failureDelegates)
                await process(context, arguments);
        }

        /// <inheritdoc/>
        public async Task OnSuccessAsync(IReadOnlyContext<TRequest> context)
        {
            if (context.IsCancelled())
                return;

            //TODO: Get standard configuration
            var arguments = new PostProcessArguments();

            foreach (var process in _successDelagates)
                await process(context, arguments);
        }
    }

    /// <summary>
    /// Executes post-processing delegates after the main pipeline operation for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class PostProcessExecution<TRequest, TResponse> : IPostProcessExecution<TRequest, TResponse>
    {
        private readonly IReadOnlyCollection<SuccessPostProcessDelegate<TRequest, TResponse>> _successDelagates;
        private readonly IReadOnlyCollection<FailurePostProcessDelegate<TRequest>> _failureDelegates;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessExecution{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="successDelagates">The collection of delegates to execute after a successful operation.</param>
        /// <param name="failureDelegates">The collection of delegates to execute after a failed operation.</param>
        public PostProcessExecution(IReadOnlyCollection<SuccessPostProcessDelegate<TRequest, TResponse>> successDelagates, IReadOnlyCollection<FailurePostProcessDelegate<TRequest>> failureDelegates)
        {
            _successDelagates = successDelagates;
            _failureDelegates = failureDelegates;
        }

        /// <inheritdoc/>
        public async Task OnFailureAsync(IReadOnlyContext<TRequest> context)
        {
            if (context.IsCancelled())
                return;

            //TODO: Get standard configuration
            var arguments = new PostProcessArguments();

            foreach (var process in _failureDelegates)
                await process(context, arguments);
        }

        /// <inheritdoc/>
        public async Task OnSuccessAsync(IReadOnlyContext<TRequest, TResponse> context)
        {
            if (context.IsCancelled())
                return;

            //TODO: Get standard configuration
            var arguments = new PostProcessArguments();

            foreach (var process in _successDelagates)
                await process.Invoke(context, arguments);
        }
    }
}