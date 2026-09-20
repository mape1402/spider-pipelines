namespace Spider.Pipelines.Web
{
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using Spider.Pipelines.Architecture;

    /// <summary>
    /// Serializes Spider architecture manifests to JSON.
    /// </summary>
    public sealed class SpiderArchitectureManifestSerializer : ISpiderArchitectureManifestSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.Default
        };

        /// <inheritdoc/>
        public string Serialize(SpiderArchitectureManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            var payload = new
            {
                producerId = manifest.ProducerId,
                manifestVersion = manifest.ManifestVersion,
                components = manifest.Components.Select(component => new
                {
                    id = component.Id,
                    kind = component.Kind,
                    displayName = component.DisplayName,
                    metadata = component.Metadata,
                    evidence = component.Evidence.Select(evidence => new
                    {
                        sourceKind = evidence.SourceKind,
                        filePath = evidence.FilePath,
                        lineNumber = evidence.LineNumber,
                        typeName = evidence.TypeName,
                        memberName = evidence.MemberName
                    }).ToArray()
                }).ToArray(),
                relations = manifest.Relations.Select(relation => new
                {
                    id = relation.Id,
                    sourceId = relation.SourceId,
                    targetId = relation.TargetId,
                    kind = relation.Kind,
                    metadata = relation.Metadata
                }).ToArray()
            };

            return JsonSerializer.Serialize(payload, Options).Replace("</", "<\\/", StringComparison.Ordinal);
        }
    }
}
