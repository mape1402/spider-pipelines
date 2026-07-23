namespace Spider.Pipelines.Extensions
{
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Parallelization;
    using Spider.Pipelines.PostProcessing;
    using Spider.Pipelines.PreProcessing;
    using Spider.Pipelines.Targeting;
    using Spider.Pipelines.Middleware;

    /// <summary>
    /// Provides concise configuration helpers for pipeline builders.
    /// </summary>
    public static class PipelineBuilderExtensions
    {
        /// <summary>
        /// Adds a preprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest> PreProcess<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            PreProcessDelegate<TRequest> handler)
            => builder.OnPreProcess(config => config.OnPreProcess(handler));

        /// <summary>
        /// Adds a preprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> PreProcess<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            PreProcessDelegate<TRequest> handler)
            => builder.OnPreProcess(config => config.OnPreProcess(handler));

        /// <summary>
        /// Adds an override target handler.
        /// </summary>
        public static IPipelineBuilder<TRequest> UseOverride<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            TargetHandler<TRequest> handler,
            OverridesConditionDelegate<TRequest> condition = null)
            => builder.OnTargeting(config => config.Overrides(handler, condition));

        /// <summary>
        /// Adds an override target handler.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> UseOverride<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            TargetHandler<TRequest, TResponse> handler,
            OverridesConditionDelegate<TRequest> condition = null)
            => builder.OnTargeting(config => config.Overrides(handler, condition));

        /// <summary>
        /// Adds middleware that wraps the target handler.
        /// </summary>
        public static IPipelineBuilder<TRequest> UseMiddleware<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            PipelineMiddlewareDelegate<TRequest> middleware)
            => builder.OnMiddleware(config => config.Use(middleware));

        /// <summary>
        /// Adds middleware that wraps the target handler.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> UseMiddleware<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            PipelineMiddlewareDelegate<TRequest, TResponse> middleware)
            => builder.OnMiddleware(config => config.Use(middleware));

        /// <summary>
        /// Adds a parallel processing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest> Parallel<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            ParallelProcessDelegate<TRequest> handler)
            => builder.OnParallel(config => config.OnParallel(handler));

        /// <summary>
        /// Adds a parallel processing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Parallel<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            ParallelProcessDelegate<TRequest> handler)
            => builder.OnParallel(config => config.OnParallel(handler));

        /// <summary>
        /// Adds a success postprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest> OnSuccess<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            SuccessPostProcessDelegate<TRequest> handler)
            => builder.OnPostProcess(config => config.OnSuccess(handler));

        /// <summary>
        /// Adds a success postprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> OnSuccess<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            SuccessPostProcessDelegate<TRequest, TResponse> handler)
            => builder.OnPostProcess(config => config.OnSuccess(handler));

        /// <summary>
        /// Adds a failure postprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest> OnFailure<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            FailurePostProcessDelegate<TRequest> handler)
            => builder.OnPostProcess(config => config.OnFailure(handler));

        /// <summary>
        /// Adds a failure postprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> OnFailure<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            FailurePostProcessDelegate<TRequest> handler)
            => builder.OnPostProcess(config => config.OnFailure(handler));
    }
}
