namespace Spider.Pipelines.Core.Internals
{
    using Spider.Pipelines.Extensions;
    using Spider.Pipelines.Parallelization;
    using Spider.Pipelines.PostProcessing;
    using Spider.Pipelines.PreProcessing;
    using Spider.Pipelines.Targeting;

    /// <summary>
    /// Represents the execution plan for a pipeline with a single request type, coordinating preprocessing, targeting, parallel, and postprocessing steps.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class ExecutionPlan<TRequest> : IExecutionPlan<TRequest>
    {
        private readonly IPreProcessExecution<TRequest> _preProcessExecution;
        private readonly ITargetExecution<TRequest> _targetExecution;
        private readonly IParallelExecution<TRequest> _parallelExecution;
        private readonly IPostProcessExecution<TRequest> _postProcessExecution;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionPlan{TRequest}"/> class.
        /// </summary>
        /// <param name="preProcessExecution">The preprocessing execution logic.</param>
        /// <param name="targetExecution">The target execution logic.</param>
        /// <param name="parallelExecution">The parallel execution logic.</param>
        /// <param name="postProcessExecution">The postprocessing execution logic.</param>
        public ExecutionPlan(IPreProcessExecution<TRequest> preProcessExecution,
                             ITargetExecution<TRequest> targetExecution,
                             IParallelExecution<TRequest> parallelExecution,
                             IPostProcessExecution<TRequest> postProcessExecution)
        {
            _preProcessExecution = preProcessExecution ?? throw new ArgumentNullException(nameof(preProcessExecution));
            _targetExecution = targetExecution ?? throw new ArgumentNullException(nameof(targetExecution));
            _parallelExecution = parallelExecution ?? throw new ArgumentNullException(nameof(parallelExecution));
            _postProcessExecution = postProcessExecution ?? throw new ArgumentNullException(nameof(postProcessExecution));
        }

        /// <inheritdoc/>
        public Task OnPreProcessAsync(IReadOnlyContext<TRequest> context)
            => _preProcessExecution.OnPreProcessAsync(context);

        /// <inheritdoc/>
        public Task OnTargetingAsync(IReadOnlyContext<TRequest> context, TargetHandler<TRequest> targetHandler)
        {
            var overrides = async () =>
            {
                var settableContext = context.AsSettable();

                try
                {
                    await _targetExecution.OnTargetExecution(context, targetHandler);
                    settableContext.Success();
                }
                catch (Exception ex)
                {
                    settableContext.Failure(ex);
                }
            };

            return Task.WhenAll(overrides(), _parallelExecution.OnParallelAsync(context));
        }

        /// <inheritdoc/>
        public Task OnPostProcessAsync(IReadOnlyContext<TRequest> context)
            => context.IsSuccess() ? _postProcessExecution.OnSuccessAsync(context) : _postProcessExecution.OnFailureAsync(context);
    }

    /// <summary>
    /// Represents the execution plan for a pipeline with a request and response type, coordinating preprocessing, targeting, parallel, and postprocessing steps.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class ExecutionPlan<TRequest, TResponse> : IExecutionPlan<TRequest, TResponse>
    {
        private readonly IPreProcessExecution<TRequest> _preProcessExecution;
        private readonly ITargetExecution<TRequest, TResponse> _targetExecution;
        private readonly IParallelExecution<TRequest, TResponse> _parallelExecution;
        private readonly IPostProcessExecution<TRequest, TResponse> _postProcessExecution;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionPlan{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="preProcessExecution">The preprocessing execution logic.</param>
        /// <param name="targetExecution">The target execution logic.</param>
        /// <param name="parallelExecution">The parallel execution logic.</param>
        /// <param name="postProcessExecution">The postprocessing execution logic.</param>
        public ExecutionPlan(IPreProcessExecution<TRequest> preProcessExecution,
                             ITargetExecution<TRequest, TResponse> targetExecution,
                             IParallelExecution<TRequest, TResponse> parallelExecution,
                             IPostProcessExecution<TRequest, TResponse> postProcessExecution)
        {
            _preProcessExecution = preProcessExecution ?? throw new ArgumentNullException(nameof(preProcessExecution));
            _targetExecution = targetExecution ?? throw new ArgumentNullException(nameof(targetExecution));
            _parallelExecution = parallelExecution ?? throw new ArgumentNullException(nameof(parallelExecution));
            _postProcessExecution = postProcessExecution ?? throw new ArgumentNullException(nameof(postProcessExecution));
        }

        /// <inheritdoc/>
        public Task OnPreProcessAsync(IReadOnlyContext<TRequest, TResponse> context)
            => _preProcessExecution.OnPreProcessAsync(context);

        /// <inheritdoc/>
        public async Task<TResponse> OnTargetingAsync(IReadOnlyContext<TRequest, TResponse> context, TargetHandler<TRequest, TResponse> targetHandler)
        {
            var overrides = async () =>
            {
                var settableContext = context.AsSettable();

                try
                {
                    var response = await _targetExecution.OnTargetExecution(context, targetHandler);
                    settableContext.Success(response);

                    return response;
                }
                catch (Exception ex)
                {
                    settableContext.Failure(ex);
                    return default;
                }

            };

            var overridesTask = overrides();

            await Task.WhenAll(overridesTask, _parallelExecution.OnParallelAsync(context));

            return overridesTask.Result;
        }

        /// <inheritdoc/>
        public Task OnPostProcessAsync(IReadOnlyContext<TRequest, TResponse> context)
            => context.IsSuccess() ? _postProcessExecution.OnSuccessAsync(context) : _postProcessExecution.OnFailureAsync(context);
    }
}
