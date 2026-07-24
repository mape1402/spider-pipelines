namespace Microsoft.Extensions.DependencyInjection
{
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Core.Internals;

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
        /// Registers a typed request-only execution boundary that wraps matching Spider pipeline executions.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TBoundary">The concrete boundary implementation type.</typeparam>
        /// <param name="builder">The Spider builder to configure.</param>
        /// <returns>The current Spider builder instance.</returns>
        public static ISpiderBuilder AddExecutionBoundary<TRequest, TBoundary>(this ISpiderBuilder builder)
            where TBoundary : class, IPipelineExecutionBoundary<TRequest>
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddScoped<IPipelineExecutionBoundary<TRequest>, TBoundary>();
            return builder;
        }

        /// <summary>
        /// Registers a typed request/response execution boundary that wraps matching Spider pipeline executions.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <typeparam name="TBoundary">The concrete boundary implementation type.</typeparam>
        /// <param name="builder">The Spider builder to configure.</param>
        /// <returns>The current Spider builder instance.</returns>
        public static ISpiderBuilder AddExecutionBoundary<TRequest, TResponse, TBoundary>(this ISpiderBuilder builder)
            where TBoundary : class, IPipelineExecutionBoundary<TRequest, TResponse>
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddScoped<IPipelineExecutionBoundary<TRequest, TResponse>, TBoundary>();
            return builder;
        }
    }
}
