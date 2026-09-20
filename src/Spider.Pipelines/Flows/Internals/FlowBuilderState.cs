namespace Spider.Pipelines.Flows.Internals
{
    using Spider.Pipelines.Architecture.Internals;

    /// <summary>
    /// Stores immutable builder state shared across typed flow builders.
    /// </summary>
    internal sealed class FlowBuilderState
    {
        private FlowBuilderState(
            string name,
            string flowId,
            string profileName,
            IReadOnlyList<IFlowStep> steps,
            SpiderArchitectureRegistry architectureRegistry,
            string lastComponentId,
            int stepIndex)
        {
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("Flow name is required.", nameof(name))
                : name;
            FlowId = flowId ?? throw new ArgumentNullException(nameof(flowId));
            ProfileName = profileName;
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
            ArchitectureRegistry = architectureRegistry ?? throw new ArgumentNullException(nameof(architectureRegistry));
            LastComponentId = lastComponentId;
            StepIndex = stepIndex;
        }

        /// <summary>
        /// Gets the logical flow name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the stable flow id.
        /// </summary>
        public string FlowId { get; }

        /// <summary>
        /// Gets the selected profile name.
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// Gets the configured steps.
        /// </summary>
        public IReadOnlyList<IFlowStep> Steps { get; }

        /// <summary>
        /// Gets the architecture metadata registry.
        /// </summary>
        public SpiderArchitectureRegistry ArchitectureRegistry { get; }

        /// <summary>
        /// Gets the last registered component id in the flow sequence.
        /// </summary>
        public string LastComponentId { get; }

        /// <summary>
        /// Gets the next step index.
        /// </summary>
        public int StepIndex { get; }

        /// <summary>
        /// Creates metadata state for a flow without a response contract.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <param name="name">The flow name.</param>
        /// <param name="architectureRegistry">The architecture registry.</param>
        /// <returns>The created state.</returns>
        public static FlowBuilderState Create<TRequest>(
            string name,
            SpiderArchitectureRegistry architectureRegistry)
        {
            var flowId = $"spider.flow:{SpiderArchitectureRegistry.Normalize(name)}";
            architectureRegistry.RegisterComponent(
                flowId,
                "spider.flow",
                name,
                new Dictionary<string, string>
                {
                    ["request"] = typeof(TRequest).FullName,
                    ["hasResponse"] = "false"
                });

            return new FlowBuilderState(name, flowId, null, Array.Empty<IFlowStep>(), architectureRegistry, null, 0);
        }

        /// <summary>
        /// Creates metadata state for a flow with a response contract.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="name">The flow name.</param>
        /// <param name="architectureRegistry">The architecture registry.</param>
        /// <returns>The created state.</returns>
        public static FlowBuilderState Create<TRequest, TResponse>(
            string name,
            SpiderArchitectureRegistry architectureRegistry)
        {
            var flowId = $"spider.flow:{SpiderArchitectureRegistry.Normalize(name)}";
            architectureRegistry.RegisterComponent(
                flowId,
                "spider.flow",
                name,
                new Dictionary<string, string>
                {
                    ["request"] = typeof(TRequest).FullName,
                    ["response"] = typeof(TResponse).FullName,
                    ["hasResponse"] = "true"
                });

            return new FlowBuilderState(name, flowId, null, Array.Empty<IFlowStep>(), architectureRegistry, null, 0);
        }

        /// <summary>
        /// Creates new state with a selected profile.
        /// </summary>
        /// <param name="profileName">The profile name.</param>
        /// <returns>The updated state.</returns>
        public FlowBuilderState WithProfile(string profileName)
        {
            var profileId = $"spider.flow-profile:{SpiderArchitectureRegistry.Normalize(profileName)}";
            ArchitectureRegistry.RegisterComponent(
                profileId,
                "spider.flow-profile",
                profileName,
                new Dictionary<string, string>());
            ArchitectureRegistry.RegisterRelation(FlowId, profileId, "uses-profile");

            return new FlowBuilderState(Name, FlowId, profileName, Steps, ArchitectureRegistry, LastComponentId, StepIndex);
        }

        /// <summary>
        /// Adds a step and records metadata.
        /// </summary>
        /// <param name="step">The runtime step.</param>
        /// <param name="stepName">The step display name.</param>
        /// <param name="kind">The step kind.</param>
        /// <param name="uses">The input types used by the step.</param>
        /// <param name="output">The output type produced by the step.</param>
        /// <param name="preservesActiveValue">A value indicating whether the step preserves the active value.</param>
        /// <returns>The updated state.</returns>
        public FlowBuilderState AddStep(
            IFlowStep step,
            string stepName,
            string kind,
            IReadOnlyCollection<Type> uses,
            Type output,
            bool preservesActiveValue)
        {
            var nextIndex = StepIndex + 1;
            var componentId = $"{FlowId}.{nextIndex:000}-{SpiderArchitectureRegistry.Normalize(stepName)}";
            ArchitectureRegistry.RegisterComponent(
                componentId,
                kind,
                stepName,
                new Dictionary<string, string>
                {
                    ["uses"] = string.Join(",", uses.Select(type => type.Name)),
                    ["output"] = output?.Name ?? string.Empty,
                    ["preservesActiveValue"] = preservesActiveValue.ToString().ToLowerInvariant()
                });

            ArchitectureRegistry.RegisterRelation(FlowId, componentId, "contains");
            if (LastComponentId != null)
                ArchitectureRegistry.RegisterRelation(LastComponentId, componentId, "next");

            return new FlowBuilderState(
                Name,
                FlowId,
                ProfileName,
                Steps.Concat(new[] { step }).ToArray(),
                ArchitectureRegistry,
                componentId,
                nextIndex);
        }
    }
}
