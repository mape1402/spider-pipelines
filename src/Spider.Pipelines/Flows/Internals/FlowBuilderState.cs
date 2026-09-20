namespace Spider.Pipelines.Flows.Internals
{
    /// <summary>
    /// Stores immutable builder state shared across typed flow builders.
    /// </summary>
    internal sealed class FlowBuilderState
    {
        public FlowBuilderState(string name)
            : this(name, null, Array.Empty<IFlowStep>())
        {
        }

        private FlowBuilderState(string name, string profileName, IReadOnlyList<IFlowStep> steps)
        {
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("Flow name is required.", nameof(name))
                : name;
            ProfileName = profileName;
            Steps = steps;
        }

        public string Name { get; }

        public string ProfileName { get; }

        public IReadOnlyList<IFlowStep> Steps { get; }

        public FlowBuilderState WithProfile(string profileName)
            => new(Name, profileName, Steps);

        public FlowBuilderState AddStep(IFlowStep step)
            => new(Name, ProfileName, Steps.Concat(new[] { step }).ToArray());
    }
}
