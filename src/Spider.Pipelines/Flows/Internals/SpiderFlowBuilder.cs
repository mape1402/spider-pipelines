namespace Spider.Pipelines.Flows.Internals
{
    using Spider.Pipelines.Flows;
    using Spider.Pipelines.RuntimeTracing;

    /// <summary>
    /// Builds and executes a composed flow without a response contract.
    /// </summary>
    /// <typeparam name="TRequest">The request type that starts the flow.</typeparam>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    internal sealed class SpiderFlowBuilder<TRequest, TCurrent> : ISpiderFlowBuilder<TRequest, TCurrent>
    {
        private readonly FlowBuilderState _state;
        private readonly ISpiderRuntimeTracer _tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderFlowBuilder{TRequest, TCurrent}"/> class.
        /// </summary>
        /// <param name="state">The immutable builder state.</param>
        /// <param name="tracer">The runtime tracer.</param>
        public SpiderFlowBuilder(FlowBuilderState state, ISpiderRuntimeTracer tracer)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _tracer = tracer;
        }

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> UsingProfile(string profileName)
            => new SpiderFlowBuilder<TRequest, TCurrent>(_state.WithProfile(profileName), _tracer);

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Describe(string description)
            => new SpiderFlowBuilder<TRequest, TCurrent>(_state.WithMetadata("description", description), _tracer);

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Tags(params string[] tags)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));

            return new SpiderFlowBuilder<TRequest, TCurrent>(_state.WithMetadata("tags", string.Join(",", tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()))), _tracer);
        }

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Metadata(string key, string value)
            => new SpiderFlowBuilder<TRequest, TCurrent>(_state.WithMetadata(key, value), _tracer);

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, TNext> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current)), descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, TNext> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current)), CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Func<TCurrent, CancellationToken, Task> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Func<TCurrent, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }, descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }, CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent, CancellationToken> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, token) =>
            {
                step(current, token);
                return Task.CompletedTask;
            }, descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent, CancellationToken> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, token) =>
            {
                step(current, token);
                return Task.CompletedTask;
            }, CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue, TNext>(Func<TValue, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value)), descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue, TNext>(Func<TValue, TNext> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value)), CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue>(step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new HistoryEffectStep<TValue>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue1, TValue2, TNext>((value1, value2, _) => Task.FromResult(step(value1, value2)), descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new HistoryTransformStep<TValue1, TValue2, TNext>((value1, value2, _) => Task.FromResult(step(value1, value2)), CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowStop otherwise)
        {
            if (otherwise == null)
                throw new ArgumentNullException(nameof(otherwise));

            return AddCurrent(new StopContinueIfStep<TCurrent>(condition));
        }

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowStop otherwise, Action<IFlowMetadataBuilder> configure)
        {
            if (otherwise == null)
                throw new ArgumentNullException(nameof(otherwise));

            return AddCurrent(new StopContinueIfStep<TCurrent>(condition, CaptureMetadata(configure)));
        }

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise)
            => AddCurrent(new ThrowContinueIfStep<TCurrent>(condition, otherwise));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ThrowContinueIfStep<TCurrent>(condition, otherwise, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext> Branch<TNext>(Action<IFlowBranchBuilder<TCurrent, TNext>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var branch = new FlowBranchBuilder<TCurrent, TNext>();
            configure(branch);
            return Add<TNext>(branch.BuildStep());
        }

        /// <inheritdoc/>
        public Task RunAsync(TRequest request, CancellationToken cancellationToken = default)
            => FlowRunner.RunAsync(_state.Name, _state.Metadata, _state.Steps, request, _tracer, cancellationToken);

        private ISpiderFlowBuilder<TRequest, TNext> Add<TNext>(IFlowStep step)
            => new SpiderFlowBuilder<TRequest, TNext>(_state.AddStep(step), _tracer);

        private ISpiderFlowBuilder<TRequest, TCurrent> AddCurrent(IFlowStep step)
            => new SpiderFlowBuilder<TRequest, TCurrent>(_state.AddStep(step), _tracer);

        private static IReadOnlyDictionary<string, string> CaptureMetadata(Action<IFlowMetadataBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = new FlowMetadataBuilder();
            configure(builder);
            return new Dictionary<string, string>(builder.MetadataValues, StringComparer.Ordinal);
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
        private readonly ISpiderRuntimeTracer _tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderFlowBuilder{TRequest, TCurrent, TResponse}"/> class.
        /// </summary>
        /// <param name="state">The immutable builder state.</param>
        /// <param name="tracer">The runtime tracer.</param>
        public SpiderFlowBuilder(FlowBuilderState state, ISpiderRuntimeTracer tracer)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _tracer = tracer;
        }

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> UsingProfile(string profileName)
            => new SpiderFlowBuilder<TRequest, TCurrent, TResponse>(_state.WithProfile(profileName), _tracer);

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Describe(string description)
            => new SpiderFlowBuilder<TRequest, TCurrent, TResponse>(_state.WithMetadata("description", description), _tracer);

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Tags(params string[] tags)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));

            return new SpiderFlowBuilder<TRequest, TCurrent, TResponse>(_state.WithMetadata("tags", string.Join(",", tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()))), _tracer);
        }

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Metadata(string key, string value)
            => new SpiderFlowBuilder<TRequest, TCurrent, TResponse>(_state.WithMetadata(key, value), _tracer);

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, TNext> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current)), descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, TNext> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>((current, _) => Task.FromResult(step(current)), CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new ActiveTransformStep<TCurrent, TNext>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Func<TCurrent, CancellationToken, Task> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Func<TCurrent, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ActiveEffectStep<TCurrent>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }, descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, _) =>
            {
                step(current);
                return Task.CompletedTask;
            }, CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent, CancellationToken> step)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, token) =>
            {
                step(current, token);
                return Task.CompletedTask;
            }, descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent, CancellationToken> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ActiveEffectStep<TCurrent>((current, token) =>
            {
                step(current, token);
                return Task.CompletedTask;
            }, CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue, TNext>(Func<TValue, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value)), descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue, TNext>(Func<TValue, TNext> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new HistoryTransformStep<TValue, TNext>((value, _) => Task.FromResult(step(value)), CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue>(step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new HistoryEffectStep<TValue>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step)
            => Add<TNext>(new HistoryTransformStep<TValue1, TValue2, TNext>((value1, value2, _) => Task.FromResult(step(value1, value2)), descriptorDelegate: step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step, Action<IFlowMetadataBuilder> configure)
            => Add<TNext>(new HistoryTransformStep<TValue1, TValue2, TNext>((value1, value2, _) => Task.FromResult(step(value1, value2)), CaptureMetadata(configure), step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new HistoryEffectStep<TValue1, TValue2>(step, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowReturn<TCurrent, TResponse> otherwise)
            => AddCurrent(new ReturnContinueIfStep<TCurrent, TResponse>(condition, otherwise));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowReturn<TCurrent, TResponse> otherwise, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ReturnContinueIfStep<TCurrent, TResponse>(condition, otherwise, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise)
            => AddCurrent(new ThrowContinueIfStep<TCurrent>(condition, otherwise));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise, Action<IFlowMetadataBuilder> configure)
            => AddCurrent(new ThrowContinueIfStep<TCurrent>(condition, otherwise, CaptureMetadata(configure)));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TNext, TResponse> Branch<TNext>(Action<IFlowBranchBuilder<TCurrent, TNext>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var branch = new FlowBranchBuilder<TCurrent, TNext>();
            configure(branch);
            return Add<TNext>(branch.BuildStep());
        }

        /// <inheritdoc/>
        public async Task<TResponse> RunAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            var state = await FlowRunner.RunAsync(_state.Name, _state.Metadata, _state.Steps, request, _tracer, cancellationToken);
            if (state.ActiveValue == null && (!typeof(TResponse).IsValueType || Nullable.GetUnderlyingType(typeof(TResponse)) != null))
                return default;

            if (state.ActiveValue is TResponse response)
                return response;

            throw new InvalidOperationException($"Flow '{_state.Name}' completed with active type {state.ActiveType.Name}, but expected {typeof(TResponse).Name}.");
        }

        private ISpiderFlowBuilder<TRequest, TNext, TResponse> Add<TNext>(IFlowStep step)
            => new SpiderFlowBuilder<TRequest, TNext, TResponse>(_state.AddStep(step), _tracer);

        private ISpiderFlowBuilder<TRequest, TCurrent, TResponse> AddCurrent(IFlowStep step)
            => new SpiderFlowBuilder<TRequest, TCurrent, TResponse>(_state.AddStep(step), _tracer);

        private static IReadOnlyDictionary<string, string> CaptureMetadata(Action<IFlowMetadataBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = new FlowMetadataBuilder();
            configure(builder);
            return new Dictionary<string, string>(builder.MetadataValues, StringComparer.Ordinal);
        }
    }
}
