namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Records a boundary named "second".
    /// </summary>
    public sealed class SecondRecordingBoundary : NamedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondRecordingBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public SecondRecordingBoundary(BoundaryEventLog log) : base("second", log) { }
    }
}
