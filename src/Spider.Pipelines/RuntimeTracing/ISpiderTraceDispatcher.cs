namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Provides runtime trace dispatcher diagnostics and synchronization hooks.
    /// </summary>
    public interface ISpiderTraceDispatcher
    {
        /// <summary>
        /// Gets the number of events dropped by the dispatcher.
        /// </summary>
        long DroppedEventCount { get; }

        /// <summary>
        /// Waits until currently queued events have been dispatched.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task FlushAsync(CancellationToken cancellationToken = default);
    }
}
