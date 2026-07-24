namespace Spider.Pipelines.Core
{
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Parallelization;
    using Spider.Pipelines.PostProcessing;
    using Spider.Pipelines.PreProcessing;
    using Spider.Pipelines.Targeting;
    using Spider.Pipelines.Middleware;

    /// <summary>
    /// Defines a contract for building pipelines with various configurations.
    /// </summary>
    public interface IPipelineBuilder
    {
        /// <summary>
        /// Specifies the request type for the pipeline.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <returns>A typed pipeline builder for the specified request type.</returns>
        IPipelineBuilder<TRequest> Typed<TRequest>();

        /// <summary>
        /// Specifies the request and response types for the pipeline.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <returns>A typed pipeline builder for the specified request and response types.</returns>
        IPipelineBuilder<TRequest, TResponse> Typed<TRequest, TResponse>();
    }

    /// <summary>
    /// Defines a contract for building and configuring a pipeline for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IPipelineBuilder<TRequest> : IPipelineBuilder
    {
        /// <summary>
        /// Configures preprocessing steps for the pipeline.
        /// </summary>
        /// <param name="before">An action to configure preprocessing.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest> OnPreProcess(Action<IPreProcessConfiguration<TRequest>> before);

        /// <summary>
        /// Configures targeting steps for the pipeline.
        /// </summary>
        /// <param name="instead">An action to configure targeting.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest> OnTargeting(Action<ITargetConfiguration<TRequest>> instead);

        /// <summary>
        /// Configures parallel processing steps for the pipeline.
        /// </summary>
        /// <param name="instead">An action to configure parallel processing.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest> OnParallel(Action<IParallelConfiguration<TRequest>> instead);

        /// <summary>
        /// Configures post-processing steps for the pipeline.
        /// </summary>
        /// <param name="after">An action to configure post-processing.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest> OnPostProcess(Action<IPostProcessConfiguration<TRequest>> after);

        /// <summary>
        /// Configures middleware steps for the pipeline.
        /// </summary>
        /// <param name="middleware">An action to configure middleware.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest> OnMiddleware(Action<IMiddlewareConfiguration<TRequest>> middleware);

        /// <summary>
        /// Adds a DI-resolved execution boundary type to this pipeline configuration.
        /// </summary>
        /// <typeparam name="TBoundary">The boundary implementation type to resolve from DI.</typeparam>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest> AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IPipelineExecutionBoundary;

        /// <summary>
        /// Adds a DI-resolved execution boundary type to this pipeline configuration.
        /// </summary>
        /// <param name="boundaryType">The boundary implementation type to resolve from DI.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest> AddExecutionBoundary(Type boundaryType);

        /// <summary>
        /// Builds the pipeline with the specified target handler.
        /// </summary>
        /// <param name="targetHandler">The target handler delegate.</param>
        /// <returns>A pipeline instance for the specified request type.</returns>
        IPipeline<TRequest> Build(TargetHandler<TRequest> targetHandler);
    }

    /// <summary>
    /// Defines a contract for building and configuring a pipeline for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IPipelineBuilder<TRequest, TResponse> : IPipelineBuilder
    {
        /// <summary>
        /// Configures preprocessing steps for the pipeline.
        /// </summary>
        /// <param name="before">An action to configure preprocessing.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest, TResponse> OnPreProcess(Action<IPreProcessConfiguration<TRequest>> before);

        /// <summary>
        /// Configures targeting steps for the pipeline.
        /// </summary>
        /// <param name="instead">An action to configure targeting.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest, TResponse> OnTargeting(Action<ITargetConfiguration<TRequest, TResponse>> instead);

        /// <summary>
        /// Configures parallel processing steps for the pipeline.
        /// </summary>
        /// <param name="instead">An action to configure parallel processing.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest, TResponse> OnParallel(Action<IParallelConfiguration<TRequest, TResponse>> instead);

        /// <summary>
        /// Configures post-processing steps for the pipeline.
        /// </summary>
        /// <param name="after">An action to configure post-processing.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest, TResponse> OnPostProcess(Action<IPostProcessConfiguration<TRequest, TResponse>> after);

        /// <summary>
        /// Configures middleware steps for the pipeline.
        /// </summary>
        /// <param name="middleware">An action to configure middleware.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest, TResponse> OnMiddleware(Action<IMiddlewareConfiguration<TRequest, TResponse>> middleware);

        /// <summary>
        /// Adds a DI-resolved execution boundary type to this pipeline configuration.
        /// </summary>
        /// <typeparam name="TBoundary">The boundary implementation type to resolve from DI.</typeparam>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest, TResponse> AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IPipelineExecutionBoundary;

        /// <summary>
        /// Adds a DI-resolved execution boundary type to this pipeline configuration.
        /// </summary>
        /// <param name="boundaryType">The boundary implementation type to resolve from DI.</param>
        /// <returns>The current pipeline builder instance.</returns>
        IPipelineBuilder<TRequest, TResponse> AddExecutionBoundary(Type boundaryType);

        /// <summary>
        /// Builds the pipeline with the specified target handler.
        /// </summary>
        /// <param name="targetHandler">The target handler delegate.</param>
        /// <returns>A pipeline instance for the specified request and response types.</returns>
        IPipeline<TRequest, TResponse> Build(TargetHandler<TRequest, TResponse> targetHandler);
    }
}
