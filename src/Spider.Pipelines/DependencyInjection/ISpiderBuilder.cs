namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines a contract for building and configuring services for the Spider pipeline.
    /// </summary>
    public interface ISpiderBuilder
    {
        /// <summary>
        /// Gets the service collection used for dependency injection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
