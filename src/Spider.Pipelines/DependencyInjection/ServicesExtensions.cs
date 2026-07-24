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
        /// Registers an execution boundary by discovering the typed boundary contracts implemented by the boundary type.
        /// </summary>
        /// <typeparam name="TBoundary">The concrete boundary implementation type.</typeparam>
        /// <param name="builder">The Spider builder to configure.</param>
        /// <returns>The current Spider builder instance.</returns>
        public static ISpiderBuilder AddExecutionBoundary<TBoundary>(this ISpiderBuilder builder)
            where TBoundary : class
            => AddExecutionBoundary(builder, typeof(TBoundary));

        /// <summary>
        /// Registers an execution boundary by discovering the typed boundary contracts implemented by the boundary type.
        /// </summary>
        /// <param name="builder">The Spider builder to configure.</param>
        /// <param name="boundaryType">The boundary implementation type to register.</param>
        /// <returns>The current Spider builder instance.</returns>
        public static ISpiderBuilder AddExecutionBoundary(this ISpiderBuilder builder, Type boundaryType)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (boundaryType == null)
                throw new ArgumentNullException(nameof(boundaryType));

            var registered = false;

            foreach (var contractType in boundaryType.GetInterfaces().Where(IsBoundaryContract))
            {
                var serviceType = contractType.ContainsGenericParameters
                    ? contractType.GetGenericTypeDefinition()
                    : contractType;

                builder.Services.AddScoped(serviceType, boundaryType);
                registered = true;
            }

            if (!registered)
                throw new InvalidOperationException($"Boundary type '{boundaryType.FullName}' must implement IBoundary<TRequest> or IBoundary<TRequest, TResponse>.");

            builder.Services.AddScoped(boundaryType, boundaryType);

            return builder;
        }

        /// <summary>
        /// Registers an execution boundary by discovering the typed boundary contracts implemented by the boundary type.
        /// </summary>
        /// <param name="builder">The Spider builder to configure.</param>
        /// <param name="boundaryType">The boundary implementation type to register.</param>
        /// <returns>The current Spider builder instance.</returns>
        public static ISpiderBuilder AddBoundary(this ISpiderBuilder builder, Type boundaryType)
            => AddExecutionBoundary(builder, boundaryType);

        /// <summary>
        /// Determines whether the specified type is a Spider execution boundary contract.
        /// </summary>
        /// <param name="contractType">The contract type to inspect.</param>
        /// <returns><c>true</c> when the type is a boundary contract; otherwise, <c>false</c>.</returns>
        private static bool IsBoundaryContract(Type contractType)
        {
            if (!contractType.IsGenericType)
                return false;

            var definition = contractType.GetGenericTypeDefinition();
            return definition == typeof(IBoundary<>) || definition == typeof(IBoundary<,>);
        }
    }
}
