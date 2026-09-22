namespace Spider.Pipelines.Flows.Internals
{
    using Spider.Pipelines.RuntimeTracing;

    /// <summary>
    /// Stores runtime state for a composed flow execution.
    /// </summary>
    internal sealed class FlowExecutionState
    {
        private readonly List<FlowHistoryEntry> _history = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowExecutionState"/> class.
        /// </summary>
        /// <param name="initialType">The declared initial request type.</param>
        /// <param name="initialValue">The initial request value.</param>
        /// <param name="tracer">The runtime tracer for the flow execution.</param>
        public FlowExecutionState(Type initialType, object initialValue, ISpiderRuntimeTracer tracer)
        {
            if (initialType == null)
                throw new ArgumentNullException(nameof(initialType));

            ActiveValue = initialValue;
            ActiveType = initialType;
            Tracer = tracer;
            AddHistory(ActiveType, initialValue, null);
        }

        /// <summary>
        /// Gets the active value.
        /// </summary>
        public object ActiveValue { get; private set; }

        /// <summary>
        /// Gets the active value type.
        /// </summary>
        public Type ActiveType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether flow execution should stop.
        /// </summary>
        public bool IsStopped { get; private set; }

        /// <summary>
        /// Gets the runtime tracer for the current flow execution.
        /// </summary>
        public ISpiderRuntimeTracer Tracer { get; }

        /// <summary>
        /// Updates the active value and appends it to history.
        /// </summary>
        /// <param name="type">The value type.</param>
        /// <param name="value">The value.</param>
        /// <param name="name">The optional history name.</param>
        public void SetActive(Type type, object value, string name = null)
        {
            ActiveType = type ?? throw new ArgumentNullException(nameof(type));
            ActiveValue = value;
            AddHistory(type, value, name);
        }

        /// <summary>
        /// Stops flow execution successfully.
        /// </summary>
        public void Stop()
            => IsStopped = true;

        /// <summary>
        /// Resolves a unique value from flow history.
        /// </summary>
        /// <typeparam name="TValue">The value type to resolve.</typeparam>
        /// <returns>The resolved value.</returns>
        public TValue Resolve<TValue>()
        {
            var exact = _history.Where(entry => entry.Type == typeof(TValue)).ToArray();
            if (exact.Length == 1)
                return (TValue)exact[0].Value;

            if (exact.Length > 1)
                throw new InvalidOperationException($"Flow history contains multiple values of type {typeof(TValue).Name}.");

            var assignable = _history.Where(entry => typeof(TValue).IsAssignableFrom(entry.Type)).ToArray();
            if (assignable.Length == 1)
                return (TValue)assignable[0].Value;

            if (assignable.Length > 1)
                throw new InvalidOperationException($"Flow history contains multiple values assignable to {typeof(TValue).Name}.");

            throw new InvalidOperationException($"Flow history does not contain a value of type {typeof(TValue).Name}.");
        }

        private void AddHistory(Type type, object value, string name)
            => _history.Add(new FlowHistoryEntry(type, value, name));

        private sealed class FlowHistoryEntry
        {
            public FlowHistoryEntry(Type type, object value, string name)
            {
                Type = type;
                Value = value;
                Name = name;
            }

            public Type Type { get; }

            public object Value { get; }

            public string Name { get; }
        }
    }
}
