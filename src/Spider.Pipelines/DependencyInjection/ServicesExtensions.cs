namespace Microsoft.Extensions.DependencyInjection
{
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Core.Internals;
    using Spider.Pipelines.Flows;

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
