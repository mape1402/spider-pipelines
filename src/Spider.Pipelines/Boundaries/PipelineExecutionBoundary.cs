namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Provides a no-op base implementation for provider-agnostic pipeline execution boundaries.
    /// </summary>
    public abstract class PipelineExecutionBoundary : IPipelineExecutionBoundary
    {
        /// <inheritdoc/>
        public virtual ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public virtual ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public virtual ValueTask FaultAsync(PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public virtual ValueTask CancelAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;
    }
}
