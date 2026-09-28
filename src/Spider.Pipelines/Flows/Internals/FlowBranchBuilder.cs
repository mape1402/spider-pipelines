namespace Spider.Pipelines.Flows.Internals
{
    using Spider.Pipelines.Flows;
    using Spider.Pipelines.RuntimeTracing;

    /// <summary>
    /// Builds branch definitions for a composed flow.
    /// </summary>
    /// <typeparam name="TCurrent">The active value type used by branch conditions.</typeparam>
    /// <typeparam name="TNext">The branch convergence type.</typeparam>
    internal sealed class FlowBranchBuilder<TCurrent, TNext> : IFlowBranchBuilder<TCurrent, TNext>
    {
        private readonly List<FlowBranchRoute<TCurrent>> _routes = new();
        private readonly Dictionary<string, string> _metadata = new(StringComparer.Ordinal);
        private FlowBranchRoute<TCurrent> _otherwise;

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> Named(string name)
        {
            CaptureMetadata(builder => builder.Named(name), _metadata);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> Describe(string description)
        {
            CaptureMetadata(builder => builder.Describe(description), _metadata);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> Tags(params string[] tags)
        {
            CaptureMetadata(builder => builder.Tags(tags), _metadata);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> Metadata(string key, string value)
        {
            CaptureMetadata(builder => builder.Metadata(key, value), _metadata);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> When(
            Func<TCurrent, bool> condition,
            Action<IFlowBranchRouteBuilder<TCurrent, TNext>> configure)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var route = new FlowBranchRouteBuilder<TCurrent, TNext>(new List<IFlowStep>());
            configure(route);
            _routes.Add(new FlowBranchRoute<TCurrent>(
                condition,
                route.DisplayNameOr("When"),
                route.MetadataValues,
                route.Steps));
            return this;
        }

        public IFlowBranchBuilder<TCurrent, TNext> Otherwise(
            Action<IFlowBranchRouteBuilder<TCurrent, TNext>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var route = new FlowBranchRouteBuilder<TCurrent, TNext>(new List<IFlowStep>());
            configure(route);
            _otherwise = new FlowBranchRoute<TCurrent>(
                _ => true,
                route.DisplayNameOr("Otherwise"),
                route.MetadataValues,
                route.Steps);
            return this;
        }

        /// <summary>
        /// Builds the executable branch step.
        /// </summary>
        /// <returns>The executable branch step.</returns>
        public IFlowStep BuildStep()
        {
            if (_otherwise == null)
                throw new InvalidOperationException("A flow branch requires an Otherwise route.");

            return new FlowBranchStep<TCurrent>(_routes, _otherwise, _metadata);
        }

        private static void CaptureMetadata(
            Action<IFlowMetadataBuilder> configure,
            IDictionary<string, string> metadata)
        {
            var builder = new FlowMetadataBuilder();
            configure(builder);
            foreach (var pair in builder.MetadataValues)
                metadata[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Builds a single branch route.
    /// </summary>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    /// <typeparam name="TBranchResult">The branch convergence type.</typeparam>
    internal sealed class FlowBranchRouteBuilder<TCurrent, TBranchResult> : IFlowBranchRouteBuilder<TCurrent, TBranchResult>
    {
        private readonly List<IFlowStep> _steps;
        private readonly Dictionary<string, string> _metadata = new(StringComparer.Ordinal);

        public FlowBranchRouteBuilder(List<IFlowStep> steps)
        {
            _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public IReadOnlyList<IFlowStep> Steps => _steps;

        public IReadOnlyDictionary<string, string> MetadataValues => _metadata;

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Named(string name)
        {
            CaptureMetadata(builder => builder.Named(name), _metadata);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Describe(string description)
        {
            CaptureMetadata(builder => builder.Describe(description), _metadata);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Tags(params string[] tags)
        {
            CaptureMetadata(builder => builder.Tags(tags), _metadata);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Metadata(string key, string value)
        {
            CaptureMetadata(builder => builder.Metadata(key, value), _metadata);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Branch<TNext>(Action<IFlowBranchBuilder<TCurrent, TNext>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var branch = new FlowBranchBuilder<TCurrent, TNext>();
            configure(branch);
            return Add<TNext>(branch.BuildStep());
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, TNext> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current)), descriptorDelegate: step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, TNext> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current)), CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Func<TCurrent, CancellationToken, Task> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Func<TCurrent, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Action<TCurrent> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }, descriptorDelegate: step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Action<TCurrent> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }, CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> ThenWith<TValue, TNext>(Func<TValue, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value)), descriptorDelegate: step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> ThenWith<TValue, TNext>(Func<TValue, TNext> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value)), CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step, CaptureMetadata(configure)));

        public string DisplayNameOr(string fallback)
            => _metadata.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : fallback;

        private IFlowBranchRouteBuilder<TNext, TBranchResult> Add<TNext>(IFlowStep step)
        {
            _steps.Add(step);
            return new FlowBranchRouteBuilder<TNext, TBranchResult>(_steps);
        }

        private IFlowBranchRouteBuilder<TCurrent, TBranchResult> AddCurrent(IFlowStep step)
        {
            _steps.Add(step);
            return this;
        }

        private static IReadOnlyDictionary<string, string> CaptureMetadata(Action<IFlowMetadataBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = new FlowMetadataBuilder();
            configure(builder);
            return new Dictionary<string, string>(builder.MetadataValues, StringComparer.Ordinal);
        }

        private static void CaptureMetadata(
            Action<IFlowMetadataBuilder> configure,
            IDictionary<string, string> metadata)
        {
            var values = CaptureMetadata(configure);
            foreach (var pair in values)
                metadata[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Represents a branch route.
    /// </summary>
    /// <typeparam name="TCurrent">The active value type used by the branch condition.</typeparam>
    internal sealed class FlowBranchRoute<TCurrent>
    {
        public FlowBranchRoute(
            Func<TCurrent, bool> condition,
            string displayName,
            IReadOnlyDictionary<string, string> metadata,
            IReadOnlyList<IFlowStep> steps)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Route" : displayName;
            Metadata = metadata == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(metadata, StringComparer.Ordinal);
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public Func<TCurrent, bool> Condition { get; }

        public string DisplayName { get; }

        public IReadOnlyDictionary<string, string> Metadata { get; }

        public IReadOnlyList<IFlowStep> Steps { get; }

        public IReadOnlyDictionary<string, string> CreateTraceMetadata()
        {
            var metadata = new Dictionary<string, string>(Metadata, StringComparer.Ordinal)
            {
                ["route"] = DisplayName
            };
            return metadata;
        }
    }

    /// <summary>
    /// Executes a branch step.
    /// </summary>
    /// <typeparam name="TCurrent">The active value type used by branch conditions.</typeparam>
    internal sealed class FlowBranchStep<TCurrent> : IFlowStep
    {
        private readonly IReadOnlyList<FlowBranchRoute<TCurrent>> _routes;
        private readonly FlowBranchRoute<TCurrent> _otherwise;

        public FlowBranchStep(
            IReadOnlyList<FlowBranchRoute<TCurrent>> routes,
            FlowBranchRoute<TCurrent> otherwise,
            IReadOnlyDictionary<string, string> metadata)
        {
            _routes = routes ?? throw new ArgumentNullException(nameof(routes));
            _otherwise = otherwise ?? throw new ArgumentNullException(nameof(otherwise));
            Descriptor = new FlowStepDescriptor(
                "Branch",
                "Branch",
                "spider.flow-branch",
                typeof(TCurrent),
                typeof(TCurrent),
                metadata);
        }

        public FlowStepDescriptor Descriptor { get; }

        public async Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
        {
            var current = (TCurrent)state.ActiveValue;
            var selected = _otherwise;

            foreach (var route in _routes)
            {
                if (!route.Condition(current))
                    continue;

                selected = route;
                break;
            }

            if (state.Tracer != null && state.Tracer.IsEnabled)
            {
                await state.Tracer.AddEventAsync(new SpiderTraceEvent
                {
                    TraceId = null,
                    SpanId = null,
                    ComponentKind = "spider.flow-branch-route",
                    DisplayName = selected.DisplayName,
                    Operation = "Branch.Selected",
                    Kind = SpiderTraceEventKind.FlowBranchSelected,
                    Status = SpiderTraceStatus.Completed,
                    Tags = FlowStepDescriptor.CreateTags(selected.Metadata),
                    Metadata = selected.CreateTraceMetadata()
                }, cancellationToken);
            }

            await FlowRunner.RunStepsAsync(selected.Steps, state, cancellationToken);
        }
    }
}
