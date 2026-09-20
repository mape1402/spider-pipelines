namespace Spider.Pipelines.Flows.Internals
{
    using Spider.Pipelines.Flows;

    /// <summary>
    /// Executes a transform step against the active value.
    /// </summary>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    /// <typeparam name="TNext">The next active value type.</typeparam>
    internal sealed class ActiveTransformStep<TCurrent, TNext> : IFlowStep
    {
        private readonly Func<TCurrent, CancellationToken, Task<TNext>> _step;

        public ActiveTransformStep(Func<TCurrent, CancellationToken, Task<TNext>> step)
        {
            _step = step ?? throw new ArgumentNullException(nameof(step));
        }

        public async Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
        {
            var result = await _step((TCurrent)state.ActiveValue, cancellationToken);
            state.SetActive(typeof(TNext), result);
        }
    }

    /// <summary>
    /// Executes a side-effect step against the active value.
    /// </summary>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    internal sealed class ActiveEffectStep<TCurrent> : IFlowStep
    {
        private readonly Func<TCurrent, CancellationToken, Task> _step;

        public ActiveEffectStep(Func<TCurrent, CancellationToken, Task> step)
        {
            _step = step ?? throw new ArgumentNullException(nameof(step));
        }

        public Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
            => _step((TCurrent)state.ActiveValue, cancellationToken);
    }

    /// <summary>
    /// Executes a transform step against one value from history.
    /// </summary>
    /// <typeparam name="TValue">The history value type.</typeparam>
    /// <typeparam name="TNext">The next active value type.</typeparam>
    internal sealed class HistoryTransformStep<TValue, TNext> : IFlowStep
    {
        private readonly Func<TValue, CancellationToken, Task<TNext>> _step;

        public HistoryTransformStep(Func<TValue, CancellationToken, Task<TNext>> step)
        {
            _step = step ?? throw new ArgumentNullException(nameof(step));
        }

        public async Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
        {
            var result = await _step(state.Resolve<TValue>(), cancellationToken);
            state.SetActive(typeof(TNext), result);
        }
    }

    /// <summary>
    /// Executes a side-effect step against one value from history.
    /// </summary>
    /// <typeparam name="TValue">The history value type.</typeparam>
    internal sealed class HistoryEffectStep<TValue> : IFlowStep
    {
        private readonly Func<TValue, CancellationToken, Task> _step;

        public HistoryEffectStep(Func<TValue, CancellationToken, Task> step)
        {
            _step = step ?? throw new ArgumentNullException(nameof(step));
        }

        public Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
            => _step(state.Resolve<TValue>(), cancellationToken);
    }

    /// <summary>
    /// Executes a transform step against two values from history.
    /// </summary>
    /// <typeparam name="TValue1">The first history value type.</typeparam>
    /// <typeparam name="TValue2">The second history value type.</typeparam>
    /// <typeparam name="TNext">The next active value type.</typeparam>
    internal sealed class HistoryTransformStep<TValue1, TValue2, TNext> : IFlowStep
    {
        private readonly Func<TValue1, TValue2, CancellationToken, Task<TNext>> _step;

        public HistoryTransformStep(Func<TValue1, TValue2, CancellationToken, Task<TNext>> step)
        {
            _step = step ?? throw new ArgumentNullException(nameof(step));
        }

        public async Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
        {
            var result = await _step(state.Resolve<TValue1>(), state.Resolve<TValue2>(), cancellationToken);
            state.SetActive(typeof(TNext), result);
        }
    }

    /// <summary>
    /// Executes a side-effect step against two values from history.
    /// </summary>
    /// <typeparam name="TValue1">The first history value type.</typeparam>
    /// <typeparam name="TValue2">The second history value type.</typeparam>
    internal sealed class HistoryEffectStep<TValue1, TValue2> : IFlowStep
    {
        private readonly Func<TValue1, TValue2, CancellationToken, Task> _step;

        public HistoryEffectStep(Func<TValue1, TValue2, CancellationToken, Task> step)
        {
            _step = step ?? throw new ArgumentNullException(nameof(step));
        }

        public Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
            => _step(state.Resolve<TValue1>(), state.Resolve<TValue2>(), cancellationToken);
    }

    /// <summary>
    /// Executes a no-response continuation guard.
    /// </summary>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    internal sealed class StopContinueIfStep<TCurrent> : IFlowStep
    {
        private readonly Func<TCurrent, bool> _condition;

        public StopContinueIfStep(Func<TCurrent, bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
        {
            if (!_condition((TCurrent)state.ActiveValue))
                state.Stop();

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Executes an exception continuation guard.
    /// </summary>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    internal sealed class ThrowContinueIfStep<TCurrent> : IFlowStep
    {
        private readonly Func<TCurrent, bool> _condition;
        private readonly FlowThrow _otherwise;

        public ThrowContinueIfStep(Func<TCurrent, bool> condition, FlowThrow otherwise)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _otherwise = otherwise ?? throw new ArgumentNullException(nameof(otherwise));
        }

        public Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
        {
            if (!_condition((TCurrent)state.ActiveValue))
                throw _otherwise.CreateException();

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Executes a response continuation guard.
    /// </summary>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    /// <typeparam name="TResponse">The flow response type.</typeparam>
    internal sealed class ReturnContinueIfStep<TCurrent, TResponse> : IFlowStep
    {
        private readonly Func<TCurrent, bool> _condition;
        private readonly FlowReturn<TCurrent, TResponse> _otherwise;

        public ReturnContinueIfStep(Func<TCurrent, bool> condition, FlowReturn<TCurrent, TResponse> otherwise)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _otherwise = otherwise ?? throw new ArgumentNullException(nameof(otherwise));
        }

        public async Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken)
        {
            var current = (TCurrent)state.ActiveValue;
            if (_condition(current))
                return;

            var response = await _otherwise.CreateResponseAsync(current, cancellationToken);
            state.SetActive(typeof(TResponse), response);
            state.Stop();
        }
    }
}
