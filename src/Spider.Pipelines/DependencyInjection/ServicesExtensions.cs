namespace Microsoft.Extensions.DependencyInjection
{
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
    }
}
