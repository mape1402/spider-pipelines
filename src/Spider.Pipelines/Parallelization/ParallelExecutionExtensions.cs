namespace Spider.Pipelines.Parallelization
{
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;

    /// <summary>
    /// Provides extension methods for executing parallel processing steps in the pipeline.
    /// </summary>
    internal static class ParallelExecutionExtensions
    {
        /// <summary>
        /// Executes a collection of parallel processing delegates asynchronously for the specified context and arguments.
        /// </summary>
        /// <typeparam name="TContext">The type of the context, implementing <see cref="IReadOnlyContext{TRequest}"/>.</typeparam>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <param name="processes">The collection of parallel processing delegates to execute.</param>
        /// <param name="arguments">The arguments to pass to each processing delegate.</param>
        /// <param name="cancellationToken">The cancellation token for the parallel operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExecuteParallelInternalAsync<TContext, TRequest>(
                                TContext context,
                                IReadOnlyCollection<ParallelProcessDelegate<TRequest>> processes,
                                ParallelProcessArguments arguments,
                                CancellationToken cancellationToken)
                                where TContext : IReadOnlyContext<TRequest>
        {
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Parallel.ForEachAsync(processes, parallelOptions, async (process, ct) =>
            {
                if (context.IsCancelled())
                    return;

                await process(context, arguments);
            });
        }
    }
}
