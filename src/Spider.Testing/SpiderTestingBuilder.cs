using Microsoft.Extensions.DependencyInjection;

namespace Spider.Testing
{
    /// <summary>
    /// Provides configuration access for Spider testing registration.
    /// </summary>
    public sealed class SpiderTestingBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderTestingBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection being configured.</param>
        public SpiderTestingBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets the service collection being configured.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
