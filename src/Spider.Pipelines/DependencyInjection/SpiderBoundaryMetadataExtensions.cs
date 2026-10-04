namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Adds architecture metadata to globally registered Spider execution boundaries.
    /// </summary>
    public static class SpiderBoundaryMetadataExtensions
    {
        /// <summary>
        /// Adds a human-friendly boundary name used by architecture tooling.
        /// </summary>
        public static ISpiderBuilder Named(this ISpiderBuilder builder, string name) => builder;

        /// <summary>
        /// Adds a boundary description used by architecture tooling.
        /// </summary>
        public static ISpiderBuilder Describe(this ISpiderBuilder builder, string description) => builder;

        /// <summary>
        /// Adds searchable boundary tags used by architecture tooling.
        /// </summary>
        public static ISpiderBuilder Tags(this ISpiderBuilder builder, params string[] tags) => builder;

        /// <summary>
        /// Adds custom boundary metadata used by architecture tooling.
        /// </summary>
        public static ISpiderBuilder Metadata(this ISpiderBuilder builder, string key, string value) => builder;

        /// <summary>
        /// Describes the boundary purpose.
        /// </summary>
        public static ISpiderBuilder Purpose(this ISpiderBuilder builder, string purpose) => builder;

        /// <summary>
        /// Groups the boundary into a module or bounded context.
        /// </summary>
        public static ISpiderBuilder Module(this ISpiderBuilder builder, string module) => builder;

        /// <summary>
        /// Describes the boundary category.
        /// </summary>
        public static ISpiderBuilder BoundaryType(this ISpiderBuilder builder, string boundaryType) => builder;

        /// <summary>
        /// Describes the entry point that crosses the boundary.
        /// </summary>
        public static ISpiderBuilder EntryPoint(this ISpiderBuilder builder, string entryPoint) => builder;

        /// <summary>
        /// Describes the protocol used at the boundary.
        /// </summary>
        public static ISpiderBuilder Protocol(this ISpiderBuilder builder, string protocol) => builder;

        /// <summary>
        /// Describes the operation exposed by the boundary.
        /// </summary>
        public static ISpiderBuilder Operation(this ISpiderBuilder builder, string operation) => builder;

        /// <summary>
        /// Describes the request and response contract crossing the boundary.
        /// </summary>
        public static ISpiderBuilder Contract<TRequest, TResponse>(this ISpiderBuilder builder) => builder;

        /// <summary>
        /// Describes security concerns for the boundary.
        /// </summary>
        public static ISpiderBuilder Security(this ISpiderBuilder builder, params string[] security) => builder;

        /// <summary>
        /// Describes policies enforced by the boundary.
        /// </summary>
        public static ISpiderBuilder Policies(this ISpiderBuilder builder, params string[] policies) => builder;

        /// <summary>
        /// Describes the expected boundary failure behavior.
        /// </summary>
        public static ISpiderBuilder FailureBehavior(this ISpiderBuilder builder, string behavior) => builder;

        /// <summary>
        /// Describes service level expectations for the boundary.
        /// </summary>
        public static ISpiderBuilder Sla(this ISpiderBuilder builder, string sla) => builder;

        /// <summary>
        /// Describes timeout expectations for the boundary.
        /// </summary>
        public static ISpiderBuilder Timeout(this ISpiderBuilder builder, string timeout) => builder;

        /// <summary>
        /// Describes observability signals emitted around the boundary.
        /// </summary>
        public static ISpiderBuilder Observability(this ISpiderBuilder builder, params string[] observability) => builder;

        /// <summary>
        /// Links the boundary to a pipeline signature.
        /// </summary>
        public static ISpiderBuilder InvokesPipeline<TRequest, TResponse>(this ISpiderBuilder builder) => builder;

        /// <summary>
        /// Links the boundary to a flow signature.
        /// </summary>
        public static ISpiderBuilder InvokesFlow<TRequest, TResponse>(this ISpiderBuilder builder) => builder;

        /// <summary>
        /// Describes an external system crossed by the boundary.
        /// </summary>
        public static ISpiderBuilder External(this ISpiderBuilder builder, string externalSystem) => builder;
    }
}
