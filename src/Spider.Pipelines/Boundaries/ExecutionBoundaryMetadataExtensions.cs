namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Adds architecture metadata to delegate execution boundary configuration.
    /// </summary>
    public static class ExecutionBoundaryMetadataExtensions
    {
        /// <summary>
        /// Adds a human-friendly boundary name used by architecture tooling.
        /// </summary>
        public static IExecutionBoundaryConfiguration Named(this IExecutionBoundaryConfiguration configuration, string name) => configuration;

        /// <summary>
        /// Adds a boundary description used by architecture tooling.
        /// </summary>
        public static IExecutionBoundaryConfiguration Describe(this IExecutionBoundaryConfiguration configuration, string description) => configuration;

        /// <summary>
        /// Adds searchable boundary tags used by architecture tooling.
        /// </summary>
        public static IExecutionBoundaryConfiguration Tags(this IExecutionBoundaryConfiguration configuration, params string[] tags) => configuration;

        /// <summary>
        /// Adds custom boundary metadata used by architecture tooling.
        /// </summary>
        public static IExecutionBoundaryConfiguration Metadata(this IExecutionBoundaryConfiguration configuration, string key, string value) => configuration;

        /// <summary>
        /// Describes the boundary purpose.
        /// </summary>
        public static IExecutionBoundaryConfiguration Purpose(this IExecutionBoundaryConfiguration configuration, string purpose) => configuration;

        /// <summary>
        /// Groups the boundary into a module or bounded context.
        /// </summary>
        public static IExecutionBoundaryConfiguration Module(this IExecutionBoundaryConfiguration configuration, string module) => configuration;

        /// <summary>
        /// Describes the boundary category.
        /// </summary>
        public static IExecutionBoundaryConfiguration BoundaryType(this IExecutionBoundaryConfiguration configuration, string boundaryType) => configuration;

        /// <summary>
        /// Describes the entry point that crosses the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration EntryPoint(this IExecutionBoundaryConfiguration configuration, string entryPoint) => configuration;

        /// <summary>
        /// Describes the protocol used at the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration Protocol(this IExecutionBoundaryConfiguration configuration, string protocol) => configuration;

        /// <summary>
        /// Describes the operation exposed by the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration Operation(this IExecutionBoundaryConfiguration configuration, string operation) => configuration;

        /// <summary>
        /// Describes the request and response contract crossing the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration Contract<TRequest, TResponse>(this IExecutionBoundaryConfiguration configuration) => configuration;

        /// <summary>
        /// Describes security concerns for the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration Security(this IExecutionBoundaryConfiguration configuration, params string[] security) => configuration;

        /// <summary>
        /// Describes policies enforced by the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration Policies(this IExecutionBoundaryConfiguration configuration, params string[] policies) => configuration;

        /// <summary>
        /// Describes the expected boundary failure behavior.
        /// </summary>
        public static IExecutionBoundaryConfiguration FailureBehavior(this IExecutionBoundaryConfiguration configuration, string behavior) => configuration;

        /// <summary>
        /// Describes service level expectations for the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration Sla(this IExecutionBoundaryConfiguration configuration, string sla) => configuration;

        /// <summary>
        /// Describes timeout expectations for the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration Timeout(this IExecutionBoundaryConfiguration configuration, string timeout) => configuration;

        /// <summary>
        /// Describes observability signals emitted around the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration Observability(this IExecutionBoundaryConfiguration configuration, params string[] observability) => configuration;

        /// <summary>
        /// Links the boundary to a pipeline signature.
        /// </summary>
        public static IExecutionBoundaryConfiguration InvokesPipeline<TRequest, TResponse>(this IExecutionBoundaryConfiguration configuration) => configuration;

        /// <summary>
        /// Links the boundary to a flow signature.
        /// </summary>
        public static IExecutionBoundaryConfiguration InvokesFlow<TRequest, TResponse>(this IExecutionBoundaryConfiguration configuration) => configuration;

        /// <summary>
        /// Describes an external system crossed by the boundary.
        /// </summary>
        public static IExecutionBoundaryConfiguration External(this IExecutionBoundaryConfiguration configuration, string externalSystem) => configuration;
    }
}
