using Spider.Pipelines.Boundaries;

namespace Spider.Testing.Internals
{
    /// <summary>
    /// Describes a discovered Spider execution boundary.
    /// </summary>
    internal sealed class BoundaryDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundaryDescriptor"/> class.
        /// </summary>
        /// <param name="boundaryType">The concrete boundary type.</param>
        public BoundaryDescriptor(Type boundaryType)
        {
            if (boundaryType == null)
                throw new ArgumentNullException(nameof(boundaryType));

            if (!typeof(IPipelineExecutionBoundary).IsAssignableFrom(boundaryType))
                throw new ArgumentException("Boundary type must implement IPipelineExecutionBoundary.", nameof(boundaryType));

            BoundaryType = boundaryType;
            Name = boundaryType.Name;
        }

        /// <summary>
        /// Gets the concrete boundary type.
        /// </summary>
        public Type BoundaryType { get; }

        /// <summary>
        /// Gets the boundary display name.
        /// </summary>
        public string Name { get; }
    }
}
