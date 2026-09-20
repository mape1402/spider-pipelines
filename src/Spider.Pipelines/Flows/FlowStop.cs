namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Represents an early successful stop for a composed flow without a response contract.
    /// </summary>
    public sealed class FlowStop
    {
        /// <summary>
        /// Gets the shared stop outcome instance.
        /// </summary>
        public static readonly FlowStop Instance = new();

        private FlowStop()
        {
        }
    }
}
