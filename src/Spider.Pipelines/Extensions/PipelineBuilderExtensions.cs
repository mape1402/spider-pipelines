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
        /// Adds a human-friendly pipeline name used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest> Named<TRequest>(this IPipelineBuilder<TRequest> builder, string name)
            => builder;

        /// <summary>
        /// Adds a human-friendly pipeline name used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Named<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string name)
            => builder;

        /// <summary>
        /// Adds a pipeline description used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest> Describe<TRequest>(this IPipelineBuilder<TRequest> builder, string description)
            => builder;

        /// <summary>
        /// Adds a pipeline description used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Describe<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string description)
            => builder;

        /// <summary>
        /// Adds searchable pipeline tags used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest> Tags<TRequest>(this IPipelineBuilder<TRequest> builder, params string[] tags)
            => builder;

        /// <summary>
        /// Adds searchable pipeline tags used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Tags<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, params string[] tags)
            => builder;

        /// <summary>
        /// Adds custom pipeline metadata used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest> Metadata<TRequest>(this IPipelineBuilder<TRequest> builder, string key, string value)
            => builder;

        /// <summary>
        /// Adds custom pipeline metadata used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Metadata<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string key, string value)
            => builder;

        /// <summary>
        /// Adds a pipeline purpose used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest> Purpose<TRequest>(this IPipelineBuilder<TRequest> builder, string purpose)
            => builder;

        /// <summary>
        /// Adds a pipeline purpose used by architecture tooling.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Purpose<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string purpose)
            => builder;

        /// <summary>
        /// Adds the trigger that starts this pipeline.
        /// </summary>
        public static IPipelineBuilder<TRequest> Trigger<TRequest>(this IPipelineBuilder<TRequest> builder, string trigger)
            => builder;

        /// <summary>
        /// Adds the trigger that starts this pipeline.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Trigger<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string trigger)
            => builder;

        /// <summary>
        /// Adds the service or operation wrapped by this pipeline.
        /// </summary>
        public static IPipelineBuilder<TRequest> Wraps<TRequest>(this IPipelineBuilder<TRequest> builder, string service)
            => builder;

        /// <summary>
        /// Adds the service or operation wrapped by this pipeline.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Wraps<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string service)
            => builder;

        /// <summary>
        /// Adds the service type wrapped by this pipeline.
        /// </summary>
        public static IPipelineBuilder<TRequest> Wraps<TRequest, TService>(this IPipelineBuilder<TRequest> builder)
            => builder;

        /// <summary>
        /// Adds the service type wrapped by this pipeline.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Wraps<TRequest, TResponse, TService>(this IPipelineBuilder<TRequest, TResponse> builder)
            => builder;

        /// <summary>
        /// Adds the pipeline input contract.
        /// </summary>
        public static IPipelineBuilder<TRequest> Input<TRequest>(this IPipelineBuilder<TRequest> builder, string input)
            => builder;

        /// <summary>
        /// Adds the pipeline input contract.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Input<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string input)
            => builder;

        /// <summary>
        /// Adds the pipeline input contract.
        /// </summary>
        public static IPipelineBuilder<TRequest> Input<TRequest, TInput>(this IPipelineBuilder<TRequest> builder)
            => builder;

        /// <summary>
        /// Adds the pipeline input contract.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Input<TRequest, TResponse, TInput>(this IPipelineBuilder<TRequest, TResponse> builder)
            => builder;

        /// <summary>
        /// Adds the pipeline output contract.
        /// </summary>
        public static IPipelineBuilder<TRequest> Output<TRequest>(this IPipelineBuilder<TRequest> builder, string output)
            => builder;

        /// <summary>
        /// Adds the pipeline output contract.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Output<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string output)
            => builder;

        /// <summary>
        /// Adds the pipeline output contract.
        /// </summary>
        public static IPipelineBuilder<TRequest> Output<TRequest, TOutput>(this IPipelineBuilder<TRequest> builder)
            => builder;

        /// <summary>
        /// Adds the pipeline output contract.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Output<TRequest, TResponse, TOutput>(this IPipelineBuilder<TRequest, TResponse> builder)
            => builder;

        /// <summary>
        /// Adds policies enforced by this pipeline.
        /// </summary>
        public static IPipelineBuilder<TRequest> Policies<TRequest>(this IPipelineBuilder<TRequest> builder, params string[] policies)
            => builder;

        /// <summary>
        /// Adds policies enforced by this pipeline.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Policies<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, params string[] policies)
            => builder;

        /// <summary>
        /// Adds the expected failure behavior.
        /// </summary>
        public static IPipelineBuilder<TRequest> FailureBehavior<TRequest>(this IPipelineBuilder<TRequest> builder, string behavior)
            => builder;

        /// <summary>
        /// Adds the expected failure behavior.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> FailureBehavior<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string behavior)
            => builder;

        /// <summary>
        /// Adds a related flow signature.
        /// </summary>
        public static IPipelineBuilder<TRequest> RelatedFlow<TRequest, TFlowRequest, TFlowResponse>(this IPipelineBuilder<TRequest> builder)
            => builder;

        /// <summary>
        /// Adds a related flow signature.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> RelatedFlow<TRequest, TResponse, TFlowRequest, TFlowResponse>(this IPipelineBuilder<TRequest, TResponse> builder)
            => builder;

        /// <summary>
        /// Adds a related flow name.
        /// </summary>
        public static IPipelineBuilder<TRequest> RelatedFlow<TRequest>(this IPipelineBuilder<TRequest> builder, string flow)
            => builder;

        /// <summary>
        /// Adds a related flow name.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> RelatedFlow<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string flow)
            => builder;

        /// <summary>
        /// Adds a module or bounded context.
        /// </summary>
        public static IPipelineBuilder<TRequest> Module<TRequest>(this IPipelineBuilder<TRequest> builder, string module)
            => builder;

        /// <summary>
        /// Adds a module or bounded context.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Module<TRequest, TResponse>(this IPipelineBuilder<TRequest, TResponse> builder, string module)
            => builder;

        /// <summary>
        /// Adds a preprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest> PreProcess<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            PreProcessDelegate<TRequest> handler)
            => builder.OnPreProcess(config => config.OnPreProcess(handler));

        /// <summary>
        /// Adds a preprocessing delegate with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest> PreProcess<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            PreProcessDelegate<TRequest> handler,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.PreProcess(handler);
        }

        /// <summary>
        /// Adds a preprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> PreProcess<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            PreProcessDelegate<TRequest> handler)
            => builder.OnPreProcess(config => config.OnPreProcess(handler));

        /// <summary>
        /// Adds a preprocessing delegate with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> PreProcess<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            PreProcessDelegate<TRequest> handler,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.PreProcess(handler);
        }

        /// <summary>
        /// Adds an override target handler.
        /// </summary>
        public static IPipelineBuilder<TRequest> UseOverride<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            TargetHandler<TRequest> handler,
            OverridesConditionDelegate<TRequest> condition = null)
            => builder.OnTargeting(config => config.Overrides(handler, condition));

        /// <summary>
        /// Adds an override target handler with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest> UseOverride<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            TargetHandler<TRequest> handler,
            OverridesConditionDelegate<TRequest> condition,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.UseOverride(handler, condition);
        }

        /// <summary>
        /// Adds an override target handler.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> UseOverride<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            TargetHandler<TRequest, TResponse> handler,
            OverridesConditionDelegate<TRequest> condition = null)
            => builder.OnTargeting(config => config.Overrides(handler, condition));

        /// <summary>
        /// Adds an override target handler with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> UseOverride<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            TargetHandler<TRequest, TResponse> handler,
            OverridesConditionDelegate<TRequest> condition,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.UseOverride(handler, condition);
        }

        /// <summary>
        /// Adds middleware that wraps the target handler.
        /// </summary>
        public static IPipelineBuilder<TRequest> UseMiddleware<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            PipelineMiddlewareDelegate<TRequest> middleware)
            => builder.OnMiddleware(config => config.Use(middleware));

        /// <summary>
        /// Adds middleware with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest> UseMiddleware<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            PipelineMiddlewareDelegate<TRequest> middleware,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.UseMiddleware(middleware);
        }

        /// <summary>
        /// Adds middleware that wraps the target handler.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> UseMiddleware<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            PipelineMiddlewareDelegate<TRequest, TResponse> middleware)
            => builder.OnMiddleware(config => config.Use(middleware));

        /// <summary>
        /// Adds middleware with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> UseMiddleware<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            PipelineMiddlewareDelegate<TRequest, TResponse> middleware,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.UseMiddleware(middleware);
        }

        /// <summary>
        /// Adds a parallel processing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest> Parallel<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            ParallelProcessDelegate<TRequest> handler)
            => builder.OnParallel(config => config.OnParallel(handler));

        /// <summary>
        /// Adds a parallel processing delegate with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest> Parallel<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            ParallelProcessDelegate<TRequest> handler,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.Parallel(handler);
        }

        /// <summary>
        /// Adds a parallel processing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Parallel<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            ParallelProcessDelegate<TRequest> handler)
            => builder.OnParallel(config => config.OnParallel(handler));

        /// <summary>
        /// Adds a parallel processing delegate with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> Parallel<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            ParallelProcessDelegate<TRequest> handler,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.Parallel(handler);
        }

        /// <summary>
        /// Adds a success postprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest> OnSuccess<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            SuccessPostProcessDelegate<TRequest> handler)
            => builder.OnPostProcess(config => config.OnSuccess(handler));

        /// <summary>
        /// Adds a success postprocessing delegate with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest> OnSuccess<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            SuccessPostProcessDelegate<TRequest> handler,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.OnSuccess(handler);
        }

        /// <summary>
        /// Adds a success postprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> OnSuccess<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            SuccessPostProcessDelegate<TRequest, TResponse> handler)
            => builder.OnPostProcess(config => config.OnSuccess(handler));

        /// <summary>
        /// Adds a success postprocessing delegate with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> OnSuccess<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            SuccessPostProcessDelegate<TRequest, TResponse> handler,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.OnSuccess(handler);
        }

        /// <summary>
        /// Adds a failure postprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest> OnFailure<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            FailurePostProcessDelegate<TRequest> handler)
            => builder.OnPostProcess(config => config.OnFailure(handler));

        /// <summary>
        /// Adds a failure postprocessing delegate with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest> OnFailure<TRequest>(
            this IPipelineBuilder<TRequest> builder,
            FailurePostProcessDelegate<TRequest> handler,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.OnFailure(handler);
        }

        /// <summary>
        /// Adds a failure postprocessing delegate.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> OnFailure<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            FailurePostProcessDelegate<TRequest> handler)
            => builder.OnPostProcess(config => config.OnFailure(handler));

        /// <summary>
        /// Adds a failure postprocessing delegate with architecture metadata.
        /// </summary>
        public static IPipelineBuilder<TRequest, TResponse> OnFailure<TRequest, TResponse>(
            this IPipelineBuilder<TRequest, TResponse> builder,
            FailurePostProcessDelegate<TRequest> handler,
            Action<IPipelineMetadataBuilder> metadata)
        {
            metadata?.Invoke(PipelineMetadataBuilder.Instance);
            return builder.OnFailure(handler);
        }
    }
}
