namespace Spider.Pipelines.PreProcessing
{
    /// <summary>
    /// Defines a contract for configuring preprocessing steps and building their execution logic for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IPreProcessConfiguration<TRequest>
    {
        /// <summary>
        /// Adds a preprocessing delegate to the configuration.
        /// </summary>
        /// <param name="handler">The preprocessing delegate to add.</param>
        /// <returns>The current configuration instance.</returns>
        /// <inheritdoc/>
        IPreProcessConfiguration<TRequest> OnPreProcess(PreProcessDelegate<TRequest> handler);

        /// <summary>
        /// Builds the execution logic for the configured preprocessing steps.
        /// </summary>
        /// <returns>An <see cref="IPreProcessExecution{TRequest}"/> instance that executes the configured preprocessing steps.</returns>
        /// <inheritdoc/>
        IPreProcessExecution<TRequest> BuildExecution();
    }
}
