namespace Spider.Pipelines.Core.Internals
{
    using Spider.Pipelines.Extensions;
    using Spider.Pipelines.Parallelization;
    using Spider.Pipelines.PostProcessing;
    using Spider.Pipelines.PreProcessing;
    using Spider.Pipelines.Targeting;
    using Spider.Pipelines.Middleware;

    /// <summary>
    /// Represents the execution plan for a pipeline with a single request type, coordinating preprocessing, targeting, parallel, and postprocessing steps.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class ExecutionPlan<TRequest> : IExecutionPlan<TRequest>
    {
        private readonly IPreProcessExecution<TRequest> _preProcessExecution;
        private readonly ITargetExecution<TRequest> _targetExecution;
        private readonly IParallelExecution<TRequest> _parallelExecution;
        private readonly IMiddlewareExecution<TRequest> _middlewareExecution;
        private readonly IPostProcessExecution<TRequest> _postProcessExecution;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionPlan{TRequest}"/> class.
        /// </summary>
        /// <param name="preProcessExecution">The preprocessing execution logic.</param>
        /// <param name="targetExecution">The target execution logic.</param>
        /// <param name="parallelExecution">The parallel execution logic.</param>
        /// <param name="middlewareExecution">The middleware execution logic.</param>
        /// <param name="postProcessExecution">The postprocessing execution logic.</param>
        public ExecutionPlan(IPreProcessExecution<TRequest> preProcessExecution,
                             ITargetExecution<TRequest> targetExecution,
                             IParallelExecution<TRequest> parallelExecution,
                             IMiddlewareExecution<TRequest> middlewareExecution,
                             IPostProcessExecution<TRequest> postProcessExecution)
        {
            _preProcessExecution = preProcessExecution ?? throw new ArgumentNullException(nameof(preProcessExecution));
            _targetExecution = targetExecution ?? throw new ArgumentNullException(nameof(targetExecution));
            _parallelExecution = parallelExecution ?? throw new ArgumentNullException(nameof(parallelExecution));
            _middlewareExecution = middlewareExecution ?? throw new ArgumentNullException(nameof(middlewareExecution));
            _postProcessExecution = postProcessExecution ?? throw new ArgumentNullException(nameof(postProcessExecution));
        }

        /// <inheritdoc/>
        public Task OnPreProcessAsync(IReadOnlyContext<TRequest> context)
            => _preProcessExecution.OnPreProcessAsync(context);

        /// <inheritdoc/>
        public async Task OnTargetingAsync(IReadOnlyContext<TRequest> context, TargetHandler<TRequest> targetHandler)
        {
            var runTarget = async () =>
            {
                var settableContext = context.AsSettable();

                try
                {
                    if (context.IsCancelled())
                    {
                        settableContext.Cancelled();
                        return;
                    }

                    await _middlewareExecution.OnMiddlewareAsync(
                        context,
                        () => _targetExecution.OnTargetExecution(context, targetHandler));

                    if (context.IsCancelled())
                        settableContext.Cancelled();
                    else
                        settableContext.Success();
                }
                catch (Exception ex)
                {
                    settableContext.Failure(ex);
                }
            };

            var runParallel = async () =>
            {
                try
                {
                    await _parallelExecution.OnParallelAsync(context);
                }
                catch (Exception ex)
                {
                    context.AsSettable().Failure(ex);
                }
            };

            switch (_parallelExecution.Mode)
            {
                case ParallelExecutionMode.BeforeTarget:
                    await runParallel();

                    if (!context.IsFailure())
                        await runTarget();

                    break;

                case ParallelExecutionMode.AfterTarget:
                    await runTarget();

                    if (!context.IsFailure() && !context.IsCancelled())
                        await runParallel();

                    break;

                default:
                    await Task.WhenAll(runTarget(), runParallel());
                    break;
            }
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
        private readonly IMiddlewareExecution<TRequest, TResponse> _middlewareExecution;
        private readonly IPostProcessExecution<TRequest, TResponse> _postProcessExecution;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionPlan{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="preProcessExecution">The preprocessing execution logic.</param>
        /// <param name="targetExecution">The target execution logic.</param>
        /// <param name="parallelExecution">The parallel execution logic.</param>
        /// <param name="middlewareExecution">The middleware execution logic.</param>
        /// <param name="postProcessExecution">The postprocessing execution logic.</param>
        public ExecutionPlan(IPreProcessExecution<TRequest> preProcessExecution,
                             ITargetExecution<TRequest, TResponse> targetExecution,
                             IParallelExecution<TRequest, TResponse> parallelExecution,
                             IMiddlewareExecution<TRequest, TResponse> middlewareExecution,
                             IPostProcessExecution<TRequest, TResponse> postProcessExecution)
        {
            _preProcessExecution = preProcessExecution ?? throw new ArgumentNullException(nameof(preProcessExecution));
            _targetExecution = targetExecution ?? throw new ArgumentNullException(nameof(targetExecution));
            _parallelExecution = parallelExecution ?? throw new ArgumentNullException(nameof(parallelExecution));
            _middlewareExecution = middlewareExecution ?? throw new ArgumentNullException(nameof(middlewareExecution));
            _postProcessExecution = postProcessExecution ?? throw new ArgumentNullException(nameof(postProcessExecution));
        }

        /// <inheritdoc/>
        public Task OnPreProcessAsync(IReadOnlyContext<TRequest, TResponse> context)
            => _preProcessExecution.OnPreProcessAsync(context);

        /// <inheritdoc/>
        public async Task<TResponse> OnTargetingAsync(IReadOnlyContext<TRequest, TResponse> context, TargetHandler<TRequest, TResponse> targetHandler)
        {
            var runTarget = async () =>
            {
                var settableContext = context.AsSettable();

                try
                {
                    if (context.IsCancelled())
                    {
                        settableContext.Cancelled();
                        return default;
                    }

                    var response = await _middlewareExecution.OnMiddlewareAsync(
                        context,
                        () => _targetExecution.OnTargetExecution(context, targetHandler));

                    if (context.IsCancelled())
                        settableContext.Cancelled();
                    else
                        settableContext.Success(response);

                    return response;
                }
                catch (Exception ex)
                {
                    settableContext.Failure(ex);
                    return default;
                }

            };

            var runParallel = async () =>
            {
                try
                {
                    await _parallelExecution.OnParallelAsync(context);
                }
                catch (Exception ex)
                {
                    context.AsSettable().Failure(ex);
                }
            };

            switch (_parallelExecution.Mode)
            {
                case ParallelExecutionMode.BeforeTarget:
                    await runParallel();
                    return context.IsFailure() ? default : await runTarget();

                case ParallelExecutionMode.AfterTarget:
                    var response = await runTarget();

                    if (!context.IsFailure() && !context.IsCancelled())
                        await runParallel();

                    return response;

                default:
                    var targetTask = runTarget();
                    await Task.WhenAll(targetTask, runParallel());
                    return targetTask.Result;
            }
        }

        /// <inheritdoc/>
        public Task OnPostProcessAsync(IReadOnlyContext<TRequest, TResponse> context)
            => context.IsSuccess() ? _postProcessExecution.OnSuccessAsync(context) : _postProcessExecution.OnFailureAsync(context);
    }
}
