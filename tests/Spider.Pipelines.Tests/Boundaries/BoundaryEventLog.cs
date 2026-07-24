namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Records boundary test events.
    /// </summary>
    public sealed class BoundaryEventLog
    {
        private readonly IList<string> _events = new List<string>();

        /// <summary>
        /// Gets a value indicating whether a test boundary is active.
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// Gets recorded events.
        /// </summary>
        public IReadOnlyCollection<string> Events => _events.ToArray();

        /// <summary>
        /// Records an event.
        /// </summary>
        /// <param name="eventName">The event name to record.</param>
        public void Add(string eventName)
            => _events.Add(eventName);

        /// <summary>
        /// Marks the boundary as active.
        /// </summary>
        public void Activate()
            => Active = true;

        /// <summary>
        /// Marks the boundary as inactive.
        /// </summary>
        public void Deactivate()
            => Active = false;
    }
}
