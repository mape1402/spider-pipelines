namespace Spider.Pipelines.Samples.Web
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Sample execution boundary used to demonstrate runtime boundary traces.
    /// </summary>
    public sealed class RuntimeSampleBoundary : PipelineExecutionBoundary
    {
        /// <inheritdoc/>
        public override ValueTask BeginAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public override ValueTask CompleteAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public override ValueTask FaultAsync(
            PipelineExecutionContext context,
            Exception exception,
            CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public override ValueTask CancelAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
            => ValueTask.CompletedTask;
    }
}
