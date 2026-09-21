namespace Spider.Pipelines.Flows.Internals
{
    using Spider.Pipelines.Flows;

    /// <summary>
    /// Builds branch definitions for a composed flow.
    /// </summary>
    /// <typeparam name="TCurrent">The active value type used by branch conditions.</typeparam>
    /// <typeparam name="TNext">The branch convergence type.</typeparam>
    internal sealed class FlowBranchBuilder<TCurrent, TNext> : IFlowBranchBuilder<TCurrent, TNext>
    {
        private readonly List<FlowBranchRoute<TCurrent>> _routes = new();
        private IReadOnlyList<IFlowStep> _otherwise;

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> Named(string name)
        {
            new FlowMetadataBuilder().Named(name);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> Describe(string description)
        {
            new FlowMetadataBuilder().Describe(description);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> Tags(params string[] tags)
        {
            new FlowMetadataBuilder().Tags(tags);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchBuilder<TCurrent, TNext> Metadata(string key, string value)
        {
            new FlowMetadataBuilder().Metadata(key, value);
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
            _routes.Add(new FlowBranchRoute<TCurrent>(condition, route.Steps));
            return this;
        }

        public IFlowBranchBuilder<TCurrent, TNext> Otherwise(
            Action<IFlowBranchRouteBuilder<TCurrent, TNext>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var route = new FlowBranchRouteBuilder<TCurrent, TNext>(new List<IFlowStep>());
            configure(route);
            _otherwise = route.Steps;
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

            return new FlowBranchStep<TCurrent>(_routes, _otherwise);
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

        public FlowBranchRouteBuilder(List<IFlowStep> steps)
        {
            _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public IReadOnlyList<IFlowStep> Steps => _steps;

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Named(string name)
        {
            new FlowMetadataBuilder().Named(name);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Describe(string description)
        {
            new FlowMetadataBuilder().Describe(description);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Tags(params string[] tags)
        {
            new FlowMetadataBuilder().Tags(tags);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Metadata(string key, string value)
        {
            new FlowMetadataBuilder().Metadata(key, value);
            return this;
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, TNext> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current))));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, TNext> step, Action<IFlowMetadataBuilder> configure)
        {
            ConfigureMetadata(configure);
            return Then(step);
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step, Action<IFlowMetadataBuilder> configure)
        {
            ConfigureMetadata(configure);
            return Then(step);
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Func<TCurrent, CancellationToken, Task> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Func<TCurrent, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
        {
            ConfigureMetadata(configure);
            return Then(step);
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Action<TCurrent> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Action<TCurrent> step, Action<IFlowMetadataBuilder> configure)
        {
            ConfigureMetadata(configure);
            return Then(step);
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> ThenWith<TValue, TNext>(Func<TValue, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value))));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TNext, TBranchResult> ThenWith<TValue, TNext>(Func<TValue, TNext> step, Action<IFlowMetadataBuilder> configure)
        {
            ConfigureMetadata(configure);
            return ThenWith<TValue, TNext>(step);
        }

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step));

        /// <inheritdoc/>
        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
        {
            ConfigureMetadata(configure);
            return ThenWith<TValue1, TValue2>(step);
        }

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

        private static void ConfigureMetadata(Action<IFlowMetadataBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            configure(new FlowMetadataBuilder());
        }
    }

    /// <summary>
    /// Represents a branch route.
    /// </summary>
    /// <typeparam name="TCurrent">The active value type used by the branch condition.</typeparam>
    internal sealed class FlowBranchRoute<TCurrent>
    {
        public FlowBranchRoute(Func<TCurrent, bool> condition, IReadOnlyList<IFlowStep> steps)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public Func<TCurrent, bool> Condition { get; }

        public IReadOnlyList<IFlowStep> Steps { get; }
    }

    /// <summary>
    /// Executes a branch step.
    /// </summary>
    /// <typeparam name="TCurrent">The active value type used by branch conditions.</typeparam>
    internal sealed class FlowBranchStep<TCurrent> : IFlowStep
    {
        private readonly IReadOnlyList<FlowBranchRoute<TCurrent>> _routes;
        private readonly IReadOnlyList<IFlowStep> _otherwise;

        public FlowBranchStep(
            IReadOnlyList<FlowBranchRoute<TCurrent>> routes,
            IReadOnlyList<IFlowStep> otherwise)
        {
            _routes = routes ?? throw new ArgumentNullException(nameof(routes));
            _otherwise = otherwise ?? throw new ArgumentNullException(nameof(otherwise));
        }

        public async Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
        {
            var current = (TCurrent)state.ActiveValue;
            var selected = _otherwise;

            foreach (var route in _routes)
            {
                if (!route.Condition(current))
                    continue;

                selected = route.Steps;
                break;
            }

            foreach (var step in selected)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (state.IsStopped)
                    break;

                await step.ExecuteAsync(state, cancellationToken);
            }
        }
    }
}
