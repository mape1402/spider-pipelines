namespace Spider.Pipelines.Samples.Basic
{
    /// <summary>
    /// Writes sample events to the console.
    /// </summary>
    public sealed class SampleEventLog
    {
        /// <summary>
        /// Writes a sample event.
        /// </summary>
        /// <param name="message">The event message.</param>
        public void Write(string message)
            => Console.WriteLine(message);
    }
}
