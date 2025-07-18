namespace Spider.Pipelines.PreProcessing
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Defines a contract for executing preprocessing logic before the main pipeline operation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IPreProcessExecution<TRequest>
    {
        /// <summary>
        /// Executes all preprocessing delegates asynchronously. Cancels the operation if any delegate sets the <c>Cancelled</c> flag in <see cref="PreProcessArguments"/>.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnPreProcessAsync(IReadOnlyContext<TRequest> context);
    }
}
