namespace Spider.Pipelines.RuntimeTracing
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Spider.Pipelines.RuntimeTracing.Stores;

    /// <summary>
    /// Configures Spider runtime tracing registrations.
    /// </summary>
    public sealed class SpiderRuntimeTracingBuilder
    {
        private bool _storeConfigured;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderRuntimeTracingBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="options">The runtime tracing options.</param>
        public SpiderRuntimeTracingBuilder(
            IServiceCollection services,
            SpiderRuntimeTracingOptions options)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the service collection being configured.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the runtime tracing options.
        /// </summary>
        public SpiderRuntimeTracingOptions Options { get; }

        /// <summary>
        /// Gets or sets the runtime tracing verbosity.
        /// </summary>
        public SpiderTraceVerbosity Verbosity
        {
            get => Options.Verbosity;
            set => Options.Verbosity = value;
        }

        /// <summary>
        /// Gets or sets the trace queue capacity.
        /// </summary>
        public int QueueCapacity
        {
            get => Options.QueueCapacity;
            set => Options.QueueCapacity = value;
        }

        /// <summary>
        /// Gets or sets the backpressure behavior.
        /// </summary>
        public SpiderTraceBackpressure Backpressure
        {
            get => Options.Backpressure;
            set => Options.Backpressure = value;
        }

        internal bool HasStoreConfigured => _storeConfigured;

        /// <summary>
        /// Uses the default in-memory runtime trace store.
        /// </summary>
        /// <param name="configure">The optional in-memory store configuration.</param>
        /// <returns>The current builder.</returns>
        public SpiderRuntimeTracingBuilder UseInMemoryStore(
            Action<InMemorySpiderTraceStoreOptions> configure = null)
        {
            var storeOptions = new InMemorySpiderTraceStoreOptions();
            configure?.Invoke(storeOptions);

            Services.RemoveAll<InMemorySpiderTraceStoreOptions>();
            Services.AddSingleton(storeOptions);
            UseStore<InMemorySpiderTraceStore>();
            return this;
        }

        /// <summary>
        /// Uses a custom runtime trace store resolved from dependency injection.
        /// </summary>
        /// <typeparam name="TStore">The store type.</typeparam>
        /// <returns>The current builder.</returns>
        public SpiderRuntimeTracingBuilder UseStore<TStore>()
            where TStore : class, ISpiderTraceStore
        {
            Services.RemoveAll<ISpiderTraceStore>();
            Services.RemoveAll<ISpiderTraceWriter>();
            Services.RemoveAll<ISpiderTraceReader>();
            Services.TryAddSingleton<TStore>();
            Services.AddSingleton<ISpiderTraceStore>(provider => provider.GetRequiredService<TStore>());
            Services.AddSingleton<ISpiderTraceWriter>(provider => provider.GetRequiredService<ISpiderTraceStore>());
            Services.AddSingleton<ISpiderTraceReader>(provider => provider.GetRequiredService<ISpiderTraceStore>());
            _storeConfigured = true;
            return this;
        }

        /// <summary>
        /// Uses a custom runtime trace store created by a factory.
        /// </summary>
        /// <param name="factory">The store factory.</param>
        /// <returns>The current builder.</returns>
        public SpiderRuntimeTracingBuilder UseStore(Func<IServiceProvider, ISpiderTraceStore> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            Services.RemoveAll<ISpiderTraceStore>();
            Services.RemoveAll<ISpiderTraceWriter>();
            Services.RemoveAll<ISpiderTraceReader>();
            Services.AddSingleton(factory);
            Services.AddSingleton<ISpiderTraceWriter>(provider => provider.GetRequiredService<ISpiderTraceStore>());
            Services.AddSingleton<ISpiderTraceReader>(provider => provider.GetRequiredService<ISpiderTraceStore>());
            _storeConfigured = true;
            return this;
        }

        /// <summary>
        /// Adds a runtime trace sink.
        /// </summary>
        /// <typeparam name="TSink">The sink type.</typeparam>
        /// <returns>The current builder.</returns>
        public SpiderRuntimeTracingBuilder AddSink<TSink>()
            where TSink : class, ISpiderTraceSink
        {
            Services.AddSingleton<ISpiderTraceSink, TSink>();
            return this;
        }

        /// <summary>
        /// Adds a runtime trace observer.
        /// </summary>
        /// <typeparam name="TObserver">The observer type.</typeparam>
        /// <returns>The current builder.</returns>
        public SpiderRuntimeTracingBuilder AddObserver<TObserver>()
            where TObserver : class, ISpiderTraceObserver
        {
            Services.AddSingleton<ISpiderTraceObserver, TObserver>();
            return this;
        }
    }
}
