namespace Spider.Pipelines.Architecture.Internals
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Stores Spider architecture metadata discovered during runtime composition.
    /// </summary>
    internal sealed class SpiderArchitectureRegistry : ISpiderArchitectureProvider
    {
        private readonly object _syncRoot = new();
        private readonly Dictionary<string, SpiderComponentDescriptor> _components = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SpiderRelationDescriptor> _relations = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers a component.
        /// </summary>
        /// <param name="id">The component id.</param>
        /// <param name="kind">The component kind.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="metadata">The metadata.</param>
        public void RegisterComponent(
            string id,
            string kind,
            string displayName,
            IReadOnlyDictionary<string, string> metadata = null)
        {
            lock (_syncRoot)
            {
                _components[id] = new SpiderComponentDescriptor(
                    id,
                    kind,
                    displayName,
                    metadata ?? new Dictionary<string, string>());
            }
        }

        /// <summary>
        /// Registers a relation.
        /// </summary>
        /// <param name="sourceId">The source component id.</param>
        /// <param name="targetId">The target component id.</param>
        /// <param name="kind">The relation kind.</param>
        /// <param name="metadata">The metadata.</param>
        public void RegisterRelation(
            string sourceId,
            string targetId,
            string kind,
            IReadOnlyDictionary<string, string> metadata = null)
        {
            var relationId = $"{kind}:{sourceId}->{targetId}";
            lock (_syncRoot)
            {
                _relations[relationId] = new SpiderRelationDescriptor(
                    relationId,
                    sourceId,
                    targetId,
                    kind,
                    metadata ?? new Dictionary<string, string>());
            }
        }

        /// <inheritdoc/>
        public SpiderArchitectureManifest GetManifest()
        {
            lock (_syncRoot)
            {
                return new SpiderArchitectureManifest(
                    _components.Values.OrderBy(component => component.Id).ToArray(),
                    _relations.Values.OrderBy(relation => relation.Id).ToArray());
            }
        }

        /// <summary>
        /// Normalizes a logical name into an architecture id segment.
        /// </summary>
        /// <param name="value">The value to normalize.</param>
        /// <returns>The normalized value.</returns>
        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unnamed";

            var normalized = Regex.Replace(value.Trim(), "([a-z0-9])([A-Z])", "$1-$2");
            normalized = Regex.Replace(normalized, "[^A-Za-z0-9]+", "-");
            return normalized.Trim('-').ToLowerInvariant();
        }
    }
}
