namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Defines runtime options for a Spider flow profile.
    /// </summary>
    public sealed class FlowProfileOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether telemetry should be emitted for flows using the profile.
        /// </summary>
        public bool TelemetryEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether metrics should be emitted for flows using the profile.
        /// </summary>
        public bool MetricsEnabled { get; set; }
    }
}
