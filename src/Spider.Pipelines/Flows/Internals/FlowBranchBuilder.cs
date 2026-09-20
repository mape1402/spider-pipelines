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

        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, TNext> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current))));

        public IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step));

        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Func<TCurrent, CancellationToken, Task> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step));

        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Action<TCurrent> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }));

        public IFlowBranchRouteBuilder<TNext, TBranchResult> ThenWith<TValue, TNext>(Func<TValue, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value))));

        public IFlowBranchRouteBuilder<TCurrent, TBranchResult> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step));

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
