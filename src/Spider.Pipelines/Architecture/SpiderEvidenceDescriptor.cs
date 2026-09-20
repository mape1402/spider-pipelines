namespace Spider.Pipelines.Architecture
{
    /// <summary>
    /// Describes source evidence for an architecture component discovered at compilation time.
    /// </summary>
    public sealed class SpiderEvidenceDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderEvidenceDescriptor"/> class.
        /// </summary>
        /// <param name="sourceKind">The evidence source kind.</param>
        /// <param name="filePath">The source file path.</param>
        /// <param name="lineNumber">The one-based source line number.</param>
        /// <param name="typeName">The containing type name.</param>
        /// <param name="memberName">The containing or referenced member name.</param>
        public SpiderEvidenceDescriptor(
            string sourceKind,
            string filePath,
            int lineNumber,
            string typeName,
            string memberName)
        {
            SourceKind = string.IsNullOrWhiteSpace(sourceKind) ? "source-generator" : sourceKind;
            FilePath = filePath ?? string.Empty;
            LineNumber = lineNumber;
            TypeName = typeName ?? string.Empty;
            MemberName = memberName ?? string.Empty;
        }

        /// <summary>
        /// Gets the evidence source kind.
        /// </summary>
        public string SourceKind { get; }

        /// <summary>
        /// Gets the source file path.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the one-based source line number.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the containing type name.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the containing or referenced member name.
        /// </summary>
        public string MemberName { get; }
    }
}
