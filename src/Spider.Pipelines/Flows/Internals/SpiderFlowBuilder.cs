namespace Spider.Pipelines.Flows.Internals
{
    using System.Reflection;
    using Spider.Pipelines.Flows;

    /// <summary>
    /// Builds and executes a composed flow without a response contract.
    /// </summary>
    /// <typeparam name="TRequest">The request type that starts the flow.</typeparam>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    internal sealed class SpiderFlowBuilder<TRequest, TCurrent> : ISpiderFlowBuilder<TRequest, TCurrent>
    {
        private readonly FlowBuilderState _state;

        public SpiderFlowBuilder(FlowBuilderState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public ISpiderFlowBuilder<TRequest, TCurrent> UsingProfile(string profileName)
            => new SpiderFlowBuilder<TRequest, TCurrent>(_state.WithProfile(profileName));

        public ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, TNext> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current))), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TNext), false);

        public ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TNext), false);

        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Func<TCurrent, CancellationToken, Task> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent, CancellationToken> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, token) =>
            {
                step(current, token);
                return Task.CompletedTask;
            }), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue, TNext>(Func<TValue, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value))), StepName(step), "spider.flow-step", new[] { typeof(TValue) }, typeof(TNext), false);

        public ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue>(step), StepName(step), "spider.flow-step", new[] { typeof(TValue) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue1, TValue2, TNext>((value1, value2, _) => Task.FromResult(step(value1, value2))), StepName(step), "spider.flow-step", new[] { typeof(TValue1), typeof(TValue2) }, typeof(TNext), false);

        public ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step), StepName(step), "spider.flow-step", new[] { typeof(TValue1), typeof(TValue2) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowStop otherwise)
        {
            if (otherwise == null)
                throw new ArgumentNullException(nameof(otherwise));

            return AddCurrent(new StopContinueIfStep<TCurrent>(condition), StepName(condition), "spider.flow-condition", new[] { typeof(TCurrent) }, typeof(TCurrent), true);
        }

        public ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise)
            => AddCurrent(new ThrowContinueIfStep<TCurrent>(condition, otherwise), StepName(condition), "spider.flow-condition", new[] { typeof(TCurrent) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TNext> Branch<TNext>(Action<IFlowBranchBuilder<TCurrent, TNext>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var branch = new FlowBranchBuilder<TCurrent, TNext>();
            configure(branch);
            return Add<TNext>(branch.BuildStep(), "Branch", "spider.flow-branch", new[] { typeof(TCurrent) }, typeof(TNext), false);
        }

        public Task RunAsync(TRequest request, CancellationToken cancellationToken = default)
            => FlowRunner.RunAsync(_state.Steps, request, cancellationToken);

        private ISpiderFlowBuilder<TRequest, TNext> Add<TNext>(IFlowStep step, string stepName, string kind, IReadOnlyCollection<Type> uses, Type output, bool preservesActiveValue)
            => new SpiderFlowBuilder<TRequest, TNext>(_state.AddStep(step, stepName, kind, uses, output, preservesActiveValue));

        private ISpiderFlowBuilder<TRequest, TCurrent> AddCurrent(IFlowStep step, string stepName, string kind, IReadOnlyCollection<Type> uses, Type output, bool preservesActiveValue)
            => new SpiderFlowBuilder<TRequest, TCurrent>(_state.AddStep(step, stepName, kind, uses, output, preservesActiveValue));

        private static string StepName(Delegate step)
            => CleanName(step?.Method);

        private static string CleanName(MethodInfo method)
        {
            if (method == null)
                return "anonymous";

            if (method.Name.Contains("<"))
                return "lambda";

            return method.Name;
        }
    }

    /// <summary>
    /// Builds and executes a composed flow with a response contract.
    /// </summary>
    /// <typeparam name="TRequest">The request type that starts the flow.</typeparam>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    /// <typeparam name="TResponse">The response type the flow must produce.</typeparam>
    internal sealed class SpiderFlowBuilder<TRequest, TCurrent, TResponse> : ISpiderFlowBuilder<TRequest, TCurrent, TResponse>
    {
        private readonly FlowBuilderState _state;

        public SpiderFlowBuilder(FlowBuilderState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> UsingProfile(string profileName)
            => new SpiderFlowBuilder<TRequest, TCurrent, TResponse>(_state.WithProfile(profileName));

        public ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, TNext> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current))), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TNext), false);

        public ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TNext), false);

        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Func<TCurrent, CancellationToken, Task> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent, CancellationToken> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, token) =>
            {
                step(current, token);
                return Task.CompletedTask;
            }), StepName(step), "spider.flow-step", new[] { typeof(TCurrent) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue, TNext>(Func<TValue, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value))), StepName(step), "spider.flow-step", new[] { typeof(TValue) }, typeof(TNext), false);

        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue>(step), StepName(step), "spider.flow-step", new[] { typeof(TValue) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue1, TValue2, TNext>((value1, value2, _) => Task.FromResult(step(value1, value2))), StepName(step), "spider.flow-step", new[] { typeof(TValue1), typeof(TValue2) }, typeof(TNext), false);

        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step), StepName(step), "spider.flow-step", new[] { typeof(TValue1), typeof(TValue2) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowReturn<TCurrent, TResponse> otherwise)
            => AddCurrent(new ReturnContinueIfStep<TCurrent, TResponse>(condition, otherwise), StepName(condition), "spider.flow-condition", new[] { typeof(TCurrent) }, typeof(TResponse), false);

        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise)
            => AddCurrent(new ThrowContinueIfStep<TCurrent>(condition, otherwise), StepName(condition), "spider.flow-condition", new[] { typeof(TCurrent) }, typeof(TCurrent), true);

        public ISpiderFlowBuilder<TRequest, TNext, TResponse> Branch<TNext>(Action<IFlowBranchBuilder<TCurrent, TNext>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var branch = new FlowBranchBuilder<TCurrent, TNext>();
            configure(branch);
            return Add<TNext>(branch.BuildStep(), "Branch", "spider.flow-branch", new[] { typeof(TCurrent) }, typeof(TNext), false);
        }

        public async Task<TResponse> RunAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            var state = await FlowRunner.RunAsync(_state.Steps, request, cancellationToken);
            if (state.ActiveValue == null && (!typeof(TResponse).IsValueType || Nullable.GetUnderlyingType(typeof(TResponse)) != null))
                return default;

            if (state.ActiveValue is TResponse response)
                return response;

            throw new InvalidOperationException($"Flow '{_state.Name}' completed with active type {state.ActiveType.Name}, but expected {typeof(TResponse).Name}.");
        }

        private ISpiderFlowBuilder<TRequest, TNext, TResponse> Add<TNext>(IFlowStep step, string stepName, string kind, IReadOnlyCollection<Type> uses, Type output, bool preservesActiveValue)
            => new SpiderFlowBuilder<TRequest, TNext, TResponse>(_state.AddStep(step, stepName, kind, uses, output, preservesActiveValue));

        private ISpiderFlowBuilder<TRequest, TCurrent, TResponse> AddCurrent(IFlowStep step, string stepName, string kind, IReadOnlyCollection<Type> uses, Type output, bool preservesActiveValue)
            => new SpiderFlowBuilder<TRequest, TCurrent, TResponse>(_state.AddStep(step, stepName, kind, uses, output, preservesActiveValue));

        private static string StepName(Delegate step)
            => CleanName(step?.Method);

        private static string CleanName(MethodInfo method)
        {
            if (method == null)
                return "anonymous";

            if (method.Name.Contains("<"))
                return "lambda";

            return method.Name;
        }
    }
}
