namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Defines read operations for runtime traces.
    /// </summary>
    public interface ISpiderTraceReader
    {
        /// <summary>
        /// Gets a runtime trace by identifier.
        /// </summary>
        /// <param name="traceId">The trace identifier.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The matching trace, or <c>null</c> when it is not found.</returns>
        ValueTask<SpiderTrace> GetAsync(
            string traceId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Queries runtime trace summaries.
        /// </summary>
        /// <param name="query">The trace query.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The matching trace summaries.</returns>
        IAsyncEnumerable<SpiderTraceSummary> QueryAsync(
            SpiderTraceQuery query,
            CancellationToken cancellationToken);
    }
}
