using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spider.Testing;
using Spider.Testing.Internals;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides dependency injection registration helpers for Spider testing.
    /// </summary>
    public static class SpiderTestingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers Spider testing services and scans assemblies for boundaries and handlers.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>A Spider testing builder.</returns>
        public static SpiderTestingBuilder AddSpiderTesting(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

            var options = new SpiderTestingOptions();
            options.AddAssemblies(assemblies);

            var registry = SpiderTestingDiscovery.Discover(options.Assemblies);

            foreach (var boundary in registry.Boundaries)
                services.TryAddTransient(boundary.BoundaryType);

            foreach (var handler in registry.Handlers)
                services.TryAddTransient(handler.ServiceType);

            services.TryAddScoped(serviceProvider => new SpiderTestingHost(
                serviceProvider,
                serviceProvider.GetRequiredService<SpiderTestingRegistry>()));
            services.TryAddScoped<ISpiderTesting>(serviceProvider => serviceProvider.GetRequiredService<SpiderTestingHost>());
            services.AddSingleton(registry);

            return new SpiderTestingBuilder(services);
        }
    }
}
