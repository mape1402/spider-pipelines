namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Controls how much runtime tracing detail Spider emits.
    /// </summary>
    public enum SpiderTraceVerbosity
    {
        /// <summary>
        /// Runtime tracing is disabled.
        /// </summary>
        Off,

        /// <summary>
        /// Emits only root execution events.
        /// </summary>
        Minimal,

        /// <summary>
        /// Emits pipelines, flows, steps, and terminal status events.
        /// </summary>
        Normal,

        /// <summary>
        /// Emits detailed boundary and branch events.
        /// </summary>
        Detailed,

        /// <summary>
        /// Emits diagnostic events for troubleshooting tracing itself.
        /// </summary>
        Diagnostic
    }
}
