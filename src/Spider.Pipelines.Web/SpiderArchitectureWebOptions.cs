namespace Spider.Pipelines.Web
{
    using Spider.Pipelines.RuntimeTracing;

    /// <summary>
    /// Configures the Spider architecture web renderer.
    /// </summary>
    public sealed class SpiderArchitectureWebOptions
    {
        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        public string Title { get; set; } = "Spider Architecture";

        /// <summary>
        /// Gets or sets a value indicating whether the raw JSON panel is available.
        /// </summary>
        public bool IncludeJsonPanel { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether source evidence is shown in the details panel.
        /// </summary>
        public bool IncludeEvidence { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether search controls are shown.
        /// </summary>
        public bool IncludeSearch { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the graph view is shown.
        /// </summary>
        public bool IncludeGraph { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether runtime trace views are shown.
        /// </summary>
        public bool IncludeRuntimeTraces { get; set; }

        /// <summary>
        /// Gets or sets the endpoint used by the browser to refresh runtime trace data.
        /// </summary>
        public string RuntimeTracesEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the initial runtime trace summaries rendered with the page.
        /// </summary>
        public IReadOnlyCollection<SpiderTraceSummary> RuntimeTraceSummaries { get; set; } = Array.Empty<SpiderTraceSummary>();

        /// <summary>
        /// Gets or sets the initial runtime traces rendered with the page.
        /// </summary>
        public IReadOnlyCollection<SpiderTrace> RuntimeTraces { get; set; } = Array.Empty<SpiderTrace>();
    }
}
