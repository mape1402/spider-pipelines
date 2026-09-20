namespace Spider.Pipelines.Core.Internals
{
    using Spider.Pipelines.Architecture.Internals;
    using Spider.Pipelines.Middleware;
    using Spider.Pipelines.Parallelization;
    using Spider.Pipelines.PostProcessing;
    using Spider.Pipelines.PreProcessing;
    using Spider.Pipelines.Targeting;

    /// <summary>
    /// Registers pipeline architecture metadata.
    /// </summary>
    internal static class PipelineArchitectureRegistrar
    {
        /// <summary>
        /// Registers metadata for a pipeline without a response contract.
        /// </summary>
        /// <typeparam name="TRequest">The request type handled by the pipeline.</typeparam>
        /// <param name="registry">The architecture registry.</param>
        /// <param name="preProcessConfiguration">The preprocess configuration.</param>
        /// <param name="middlewareConfiguration">The middleware configuration.</param>
        /// <param name="targetConfiguration">The target configuration.</param>
        /// <param name="parallelConfiguration">The parallel configuration.</param>
        /// <param name="postProcessConfiguration">The postprocess configuration.</param>
        public static void Register<TRequest>(
            SpiderArchitectureRegistry registry,
            PreProcessConfiguration<TRequest> preProcessConfiguration,
            MiddlewareConfiguration<TRequest> middlewareConfiguration,
            TargetConfiguration<TRequest> targetConfiguration,
            ParallelConfiguration<TRequest> parallelConfiguration,
            PostProcessConfiguration<TRequest> postProcessConfiguration)
        {
            RegisterPipeline(
                registry,
                typeof(TRequest),
                null,
                preProcessConfiguration.Count,
                middlewareConfiguration.Count,
                targetConfiguration.HasOverride,
                parallelConfiguration.Count,
                postProcessConfiguration.SuccessCount,
                postProcessConfiguration.FailureCount);
        }

        /// <summary>
        /// Registers metadata for a pipeline with a response contract.
        /// </summary>
        /// <typeparam name="TRequest">The request type handled by the pipeline.</typeparam>
        /// <typeparam name="TResponse">The response type returned by the pipeline.</typeparam>
        /// <param name="registry">The architecture registry.</param>
        /// <param name="preProcessConfiguration">The preprocess configuration.</param>
        /// <param name="middlewareConfiguration">The middleware configuration.</param>
        /// <param name="targetConfiguration">The target configuration.</param>
        /// <param name="parallelConfiguration">The parallel configuration.</param>
        /// <param name="postProcessConfiguration">The postprocess configuration.</param>
        public static void Register<TRequest, TResponse>(
            SpiderArchitectureRegistry registry,
            PreProcessConfiguration<TRequest> preProcessConfiguration,
            MiddlewareConfiguration<TRequest, TResponse> middlewareConfiguration,
            TargetConfiguration<TRequest, TResponse> targetConfiguration,
            ParallelConfiguration<TRequest, TResponse> parallelConfiguration,
            PostProcessConfiguration<TRequest, TResponse> postProcessConfiguration)
        {
            RegisterPipeline(
                registry,
                typeof(TRequest),
                typeof(TResponse),
                preProcessConfiguration.Count,
                middlewareConfiguration.Count,
                targetConfiguration.HasOverride,
                parallelConfiguration.Count,
                postProcessConfiguration.SuccessCount,
                postProcessConfiguration.FailureCount);
        }

        private static void RegisterPipeline(
            SpiderArchitectureRegistry registry,
            Type requestType,
            Type responseType,
            int preProcessCount,
            int middlewareCount,
            bool hasTargetOverride,
            int parallelCount,
            int postSuccessCount,
            int postFailureCount)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var pipelineId = BuildPipelineId(requestType, responseType);
            var pipelineMetadata = new Dictionary<string, string>
            {
                ["request"] = requestType.FullName,
                ["hasResponse"] = (responseType != null).ToString()
            };

            if (responseType != null)
                pipelineMetadata["response"] = responseType.FullName;

            registry.RegisterComponent(pipelineId, "spider.pipeline", requestType.Name, pipelineMetadata);

            RegisterStage(registry, pipelineId, "pre-process", "Pre-process", preProcessCount, 1);
            RegisterStage(registry, pipelineId, "middleware", "Middleware", middlewareCount, 2);
            RegisterStage(registry, pipelineId, "target", "Target", 1, 3, new Dictionary<string, string>
            {
                ["hasOverride"] = hasTargetOverride.ToString()
            });
            RegisterStage(registry, pipelineId, "parallel", "Parallel", parallelCount, 4);
            RegisterStage(registry, pipelineId, "post-success", "Post-process success", postSuccessCount, 5);
            RegisterStage(registry, pipelineId, "post-failure", "Post-process failure", postFailureCount, 6);
        }

        private static void RegisterStage(
            SpiderArchitectureRegistry registry,
            string pipelineId,
            string stageName,
            string displayName,
            int count,
            int order,
            IReadOnlyDictionary<string, string> metadata = null)
        {
            var stageId = $"{pipelineId}.{stageName}";
            var stageMetadata = new Dictionary<string, string>
            {
                ["stage"] = stageName,
                ["count"] = count.ToString(),
                ["order"] = order.ToString()
            };

            if (metadata != null)
            {
                foreach (var pair in metadata)
                    stageMetadata[pair.Key] = pair.Value;
            }

            registry.RegisterComponent(stageId, "spider.pipeline-stage", displayName, stageMetadata);
            registry.RegisterRelation(pipelineId, stageId, "contains");
        }

        private static string BuildPipelineId(Type requestType, Type responseType)
        {
            var requestName = SpiderArchitectureRegistry.Normalize(requestType.Name);
            if (responseType == null)
                return $"spider.pipeline:{requestName}";

            var responseName = SpiderArchitectureRegistry.Normalize(responseType.Name);
            return $"spider.pipeline:{requestName}-to-{responseName}";
        }
    }
}
