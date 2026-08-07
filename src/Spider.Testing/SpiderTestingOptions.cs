using System.Reflection;

namespace Spider.Testing
{
    /// <summary>
    /// Stores assembly discovery options for Spider testing.
    /// </summary>
    public sealed class SpiderTestingOptions
    {
        private readonly List<Assembly> _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderTestingOptions"/> class.
        /// </summary>
        public SpiderTestingOptions()
        {
            _assemblies = new List<Assembly>();
        }

        /// <summary>
        /// Gets the assemblies scanned for boundaries and handlers.
        /// </summary>
        public IReadOnlyList<Assembly> Assemblies => _assemblies;

        /// <summary>
        /// Adds assemblies to the discovery list.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        public void AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            foreach (var assembly in assemblies)
            {
                if (assembly == null)
                    throw new ArgumentException("Assembly entries cannot be null.", nameof(assemblies));

                if (!_assemblies.Contains(assembly))
                    _assemblies.Add(assembly);
            }
        }
    }
}
