namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Records a boundary named "first".
    /// </summary>
    public sealed class FirstRecordingBoundary : NamedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstRecordingBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public FirstRecordingBoundary(BoundaryEventLog log) : base("first", log) { }
    }
}
