namespace Spider.Testing.Internals
{
    /// <summary>
    /// Stores discovered Spider testing components.
    /// </summary>
    internal sealed class SpiderTestingRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderTestingRegistry"/> class.
        /// </summary>
        /// <param name="boundaries">The discovered boundary descriptors.</param>
        /// <param name="handlers">The discovered handler descriptors.</param>
        public SpiderTestingRegistry(
            IReadOnlyList<BoundaryDescriptor> boundaries,
            IReadOnlyList<HandlerDescriptor> handlers)
        {
            Boundaries = boundaries ?? throw new ArgumentNullException(nameof(boundaries));
            Handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        }

        /// <summary>
        /// Gets the discovered boundary descriptors.
        /// </summary>
        public IReadOnlyList<BoundaryDescriptor> Boundaries { get; }

        /// <summary>
        /// Gets the discovered handler descriptors.
        /// </summary>
        public IReadOnlyList<HandlerDescriptor> Handlers { get; }
    }
}
