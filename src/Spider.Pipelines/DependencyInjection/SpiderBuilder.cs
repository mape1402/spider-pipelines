namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides a builder for configuring services for the Spider pipeline.
    /// </summary>
    internal class SpiderBuilder : ISpiderBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection used for dependency injection.</param>
        public SpiderBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc/>
        public IServiceCollection Services { get; }
    }
}
