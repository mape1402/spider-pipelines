namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Records a generic boundary named "boundary".
    /// </summary>
    public sealed class RecordingBoundary : NamedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public RecordingBoundary(BoundaryEventLog log) : base("boundary", log) { }
    }
}
