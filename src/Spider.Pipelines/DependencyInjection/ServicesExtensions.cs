namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Core.Internals;
    using Spider.Pipelines.Flows;
    using Spider.Pipelines.RuntimeTracing;
    using Spider.Pipelines.RuntimeTracing.Internals;

    /// <summary>
    /// Provides extension methods for registering Spider pipeline services with the dependency injection container.
    /// </summary>
    public static class ServicesExtensions
    {
        /// <summary>
        /// Adds Spider pipeline services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add the Spider services to.</param>
        /// <returns>An <see cref="ISpiderBuilder"/> for further configuration.</returns>
        public static ISpiderBuilder AddSpider(this IServiceCollection services)
        {
            services.AddScoped<ISpider, InternalSpider>();
            services.AddScoped(typeof(IServiceBridge<>), typeof(ServiceBridge<>));
            services.TryAddSingleton<ISpiderTraceContextAccessor, SpiderTraceContextAccessor>();
            services.TryAddSingleton<ISpiderRuntimeTracer>(NullSpiderRuntimeTracer.Instance);
            services.AddSingleton(provider =>
            {
                var registry = new FlowProfileRegistry();
                foreach (var registration in provider.GetServices<FlowProfileRegistration>())
                    registry.Set(registration.Name, registration.Options);

                return registry;
            });
            return new SpiderBuilder(services);
        }

        /// <summary>
        /// Adds Spider pipeline services to the specified <see cref="IServiceCollection"/> and applies builder configuration.
        /// </summary>
        /// <param name="services">The service collection to add the Spider services to.</param>
        /// <param name="configure">The configuration action to apply to the Spider builder.</param>
        /// <returns>An <see cref="ISpiderBuilder"/> for further configuration.</returns>
        public static ISpiderBuilder AddSpider(this IServiceCollection services, Action<ISpiderBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = services.AddSpider();
            configure(builder);
            return builder;
        }

        /// <summary>
        /// Adds Spider runtime tracing services with the default in-memory trace store.
        /// </summary>
        /// <param name="services">The service collection to add runtime tracing services to.</param>
        /// <returns>The runtime tracing builder.</returns>
        public static SpiderRuntimeTracingBuilder AddSpiderRuntimeTracing(this IServiceCollection services)
            => AddSpiderRuntimeTracing(services, null);

        /// <summary>
        /// Adds Spider runtime tracing services and applies runtime tracing configuration.
        /// </summary>
        /// <param name="services">The service collection to add runtime tracing services to.</param>
        /// <param name="configure">The runtime tracing configuration.</param>
        /// <returns>The runtime tracing builder.</returns>
        public static SpiderRuntimeTracingBuilder AddSpiderRuntimeTracing(
            this IServiceCollection services,
            Action<SpiderRuntimeTracingBuilder> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.RemoveAll<ISpiderRuntimeTracer>();
            services.RemoveAll<ISpiderTraceDispatcher>();
            services.RemoveAll<ISpiderTraceLiveStream>();
            services.RemoveAll<SpiderRuntimeTracingOptions>();

            var options = new SpiderRuntimeTracingOptions();
            var builder = new SpiderRuntimeTracingBuilder(services, options);
            configure?.Invoke(builder);

            if (!builder.HasStoreConfigured)
                builder.UseInMemoryStore();

            services.AddSingleton(options);
            services.TryAddSingleton<ISpiderTraceContextAccessor, SpiderTraceContextAccessor>();
            services.AddSingleton<ISpiderTraceLiveStream, InMemorySpiderTraceLiveStream>();
            services.AddSingleton<SpiderRuntimeTracer>();
            services.AddSingleton<ISpiderRuntimeTracer>(provider => provider.GetRequiredService<SpiderRuntimeTracer>());
            services.AddSingleton<ISpiderTraceDispatcher>(provider => provider.GetRequiredService<SpiderRuntimeTracer>());

            return builder;
        }

        /// <summary>
        /// Registers an execution boundary that wraps full Spider pipeline executions.
        /// </summary>
        /// <typeparam name="TBoundary">The concrete boundary implementation type.</typeparam>
        /// <param name="builder">The Spider builder to configure.</param>
        /// <returns>The current Spider builder instance.</returns>
        public static ISpiderBuilder AddExecutionBoundary<TBoundary>(this ISpiderBuilder builder)
            where TBoundary : class, IPipelineExecutionBoundary
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddScoped<IPipelineExecutionBoundary, TBoundary>();
            return builder;
        }

        /// <summary>
        /// Registers a reusable flow profile.
        /// </summary>
        /// <param name="builder">The Spider builder to configure.</param>
        /// <param name="name">The profile name.</param>
        /// <param name="configure">The profile configuration.</param>
        /// <returns>The current Spider builder instance.</returns>
        public static ISpiderBuilder AddFlowProfile(
            this ISpiderBuilder builder,
            string name,
            Action<FlowProfileOptions> configure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var options = new FlowProfileOptions();
            configure(options);
            builder.Services.AddSingleton(new FlowProfileRegistration(name, options));
            return builder;
        }
    }
}
