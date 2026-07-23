namespace Spider.Pipelines.Core.Internals
{
    using Spider.Pipelines.Parallelization;
    using Spider.Pipelines.PostProcessing;
    using Spider.Pipelines.PreProcessing;
    using Spider.Pipelines.Targeting;

    /// <summary>
    /// Provides a base implementation for pipeline builders, supporting type-safe configuration.
    /// </summary>
    internal abstract class PipelineBuilder : IPipelineBuilder
    {
        /// <inheritdoc/>
        public IPipelineBuilder<TRequest> Typed<TRequest>()
        {
            if (this is IPipelineBuilder<TRequest> output)
                return output;

            throw new NotSupportedException("Current pipeline builder cannot be casted.");
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest, TResponse> Typed<TRequest, TResponse>()
        {
            if (this is IPipelineBuilder<TRequest, TResponse> output)
                return output;

            throw new NotSupportedException("Current pipeline builder cannot be casted.");
        }
    }

    /// <summary>
    /// Provides a builder for configuring and constructing a pipeline for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class PipelineBuilder<TRequest> : PipelineBuilder, IPipelineBuilder<TRequest>
    {
        private readonly IPreProcessConfiguration<TRequest> _preProcessConfiguration;
        private readonly IPostProcessConfiguration<TRequest> _postProcessConfiguration;
        private readonly ITargetConfiguration<TRequest> _targetConfiguration;
        private readonly IParallelConfiguration<TRequest> _parallelConfiguration;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineBuilder{TRequest}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _preProcessConfiguration = new PreProcessConfiguration<TRequest>(_serviceProvider);
            _postProcessConfiguration = new PostProcessConfiguration<TRequest>(_serviceProvider);
            _targetConfiguration = new TargetConfiguration<TRequest>(_serviceProvider);
            _parallelConfiguration = new ParallelConfiguration<TRequest>(_serviceProvider);
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest> OnPreProcess(Action<IPreProcessConfiguration<TRequest>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config(_preProcessConfiguration);
            return this;
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest> OnTargeting(Action<ITargetConfiguration<TRequest>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config(_targetConfiguration);
            return this;
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest> OnParallel(Action<IParallelConfiguration<TRequest>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config(_parallelConfiguration);
            return this;
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest> OnPostProcess(Action<IPostProcessConfiguration<TRequest>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config(_postProcessConfiguration);
            return this;
        }

        /// <inheritdoc/>
        public IPipeline<TRequest> Build(TargetHandler<TRequest> targetHandler)
        {
            if (targetHandler == null)
                throw new ArgumentNullException(nameof(targetHandler));

            var preProcessExecution = _preProcessConfiguration.BuildExecution();
            var targetExecution = _targetConfiguration.BuildExecution();
            var parallelExecution = _parallelConfiguration.BuildExecution();
            var postProcessExecution = _postProcessConfiguration.BuildExecution();

            var executionPlan = new ExecutionPlan<TRequest>(preProcessExecution, targetExecution, parallelExecution, postProcessExecution);

            return new Pipeline<TRequest>(targetHandler, executionPlan, _serviceProvider);
        }
    }

    /// <summary>
    /// Provides a builder for configuring and constructing a pipeline for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class PipelineBuilder<TRequest, TResponse> : PipelineBuilder, IPipelineBuilder<TRequest, TResponse>
    {
        private readonly IPreProcessConfiguration<TRequest> _preProcessConfiguration;
        private readonly IPostProcessConfiguration<TRequest, TResponse> _postProcessConfiguration;
        private readonly ITargetConfiguration<TRequest, TResponse> _targetConfiguration;
        private readonly IParallelConfiguration<TRequest, TResponse> _parallelConfiguration;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineBuilder{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _preProcessConfiguration = new PreProcessConfiguration<TRequest>(_serviceProvider);
            _postProcessConfiguration = new PostProcessConfiguration<TRequest, TResponse>(_serviceProvider);
            _targetConfiguration = new TargetConfiguration<TRequest, TResponse>(_serviceProvider);
            _parallelConfiguration = new ParallelConfiguration<TRequest, TResponse>(_serviceProvider);
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest, TResponse> OnPreProcess(Action<IPreProcessConfiguration<TRequest>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config(_preProcessConfiguration);
            return this;
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest, TResponse> OnTargeting(Action<ITargetConfiguration<TRequest, TResponse>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config(_targetConfiguration);
            return this;
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest, TResponse> OnParallel(Action<IParallelConfiguration<TRequest, TResponse>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config(_parallelConfiguration);
            return this;
        }

        /// <inheritdoc/>
        public IPipelineBuilder<TRequest, TResponse> OnPostProcess(Action<IPostProcessConfiguration<TRequest, TResponse>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config(_postProcessConfiguration);
            return this;
        }

        /// <inheritdoc/>
        public IPipeline<TRequest, TResponse> Build(TargetHandler<TRequest, TResponse> targetHandler)
        {
            if (targetHandler == null)
                throw new ArgumentNullException(nameof(targetHandler));

            var preProcessExecution = _preProcessConfiguration.BuildExecution();
            var targetExecution = _targetConfiguration.BuildExecution();
            var parallelExecution = _parallelConfiguration.BuildExecution();
            var postProcessExecution = _postProcessConfiguration.BuildExecution();

            var executionPlan = new ExecutionPlan<TRequest, TResponse>(preProcessExecution, targetExecution, parallelExecution, postProcessExecution);

            return new Pipeline<TRequest, TResponse>(targetHandler, executionPlan, _serviceProvider);
        }
    }
}
