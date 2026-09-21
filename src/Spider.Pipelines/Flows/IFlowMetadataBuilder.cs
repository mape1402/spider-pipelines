namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Defines descriptive metadata for a flow component.
    /// </summary>
    public interface IFlowMetadataBuilder
    {
        /// <summary>
        /// Assigns a display name to the documented flow component.
        /// </summary>
        /// <param name="name">The display name to show in generated documentation.</param>
        /// <returns>The current metadata builder.</returns>
        IFlowMetadataBuilder Named(string name);

        /// <summary>
        /// Assigns a short description to the documented flow component.
        /// </summary>
        /// <param name="description">The description to show in generated documentation.</param>
        /// <returns>The current metadata builder.</returns>
        IFlowMetadataBuilder Describe(string description);

        /// <summary>
        /// Adds descriptive tags to the documented flow component.
        /// </summary>
        /// <param name="tags">The tags to show in generated documentation.</param>
        /// <returns>The current metadata builder.</returns>
        IFlowMetadataBuilder Tags(params string[] tags);

        /// <summary>
        /// Adds custom key-value metadata to the documented flow component.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The current metadata builder.</returns>
        IFlowMetadataBuilder Metadata(string key, string value);
    }
}
