namespace Spider.Pipelines.Extensions
{
    /// <summary>
    /// Describes pipeline, stage, and boundary metadata without changing runtime execution.
    /// </summary>
    public interface IPipelineMetadataBuilder
    {
        /// <summary>
        /// Sets a human-friendly name.
        /// </summary>
        IPipelineMetadataBuilder Named(string name);

        /// <summary>
        /// Describes what the documented item does.
        /// </summary>
        IPipelineMetadataBuilder Describe(string description);

        /// <summary>
        /// Adds searchable tags.
        /// </summary>
        IPipelineMetadataBuilder Tags(params string[] tags);

        /// <summary>
        /// Adds custom metadata.
        /// </summary>
        IPipelineMetadataBuilder Metadata(string key, string value);

        /// <summary>
        /// Describes the business or technical purpose.
        /// </summary>
        IPipelineMetadataBuilder Purpose(string purpose);

        /// <summary>
        /// Describes what starts the item.
        /// </summary>
        IPipelineMetadataBuilder Trigger(string trigger);

        /// <summary>
        /// Describes a wrapped service or operation.
        /// </summary>
        IPipelineMetadataBuilder Wraps(string service);

        /// <summary>
        /// Describes a wrapped service type.
        /// </summary>
        IPipelineMetadataBuilder Wraps<TService>();

        /// <summary>
        /// Describes the input contract.
        /// </summary>
        IPipelineMetadataBuilder Input<TInput>();

        /// <summary>
        /// Describes the output contract.
        /// </summary>
        IPipelineMetadataBuilder Output<TOutput>();

        /// <summary>
        /// Describes policies enforced by the item.
        /// </summary>
        IPipelineMetadataBuilder Policies(params string[] policies);

        /// <summary>
        /// Describes expected failure behavior.
        /// </summary>
        IPipelineMetadataBuilder FailureBehavior(string behavior);

        /// <summary>
        /// Links to a related flow by name.
        /// </summary>
        IPipelineMetadataBuilder RelatedFlow(string flow);

        /// <summary>
        /// Links to a related flow signature.
        /// </summary>
        IPipelineMetadataBuilder RelatedFlow<TRequest, TResponse>();

        /// <summary>
        /// Groups the item into a module or bounded context.
        /// </summary>
        IPipelineMetadataBuilder Module(string module);

        /// <summary>
        /// Describes the boundary category.
        /// </summary>
        IPipelineMetadataBuilder BoundaryType(string boundaryType);

        /// <summary>
        /// Describes the entry point that crosses a boundary.
        /// </summary>
        IPipelineMetadataBuilder EntryPoint(string entryPoint);

        /// <summary>
        /// Describes the protocol used at the boundary.
        /// </summary>
        IPipelineMetadataBuilder Protocol(string protocol);

        /// <summary>
        /// Describes the operation exposed by the boundary.
        /// </summary>
        IPipelineMetadataBuilder Operation(string operation);

        /// <summary>
        /// Describes the request and response contract exposed by a boundary.
        /// </summary>
        IPipelineMetadataBuilder Contract<TRequest, TResponse>();

        /// <summary>
        /// Describes security concerns for a boundary.
        /// </summary>
        IPipelineMetadataBuilder Security(params string[] security);

        /// <summary>
        /// Describes service level expectations.
        /// </summary>
        IPipelineMetadataBuilder Sla(string sla);

        /// <summary>
        /// Describes timeout expectations.
        /// </summary>
        IPipelineMetadataBuilder Timeout(string timeout);

        /// <summary>
        /// Describes observability signals emitted by the item.
        /// </summary>
        IPipelineMetadataBuilder Observability(params string[] observability);

        /// <summary>
        /// Links a boundary to an invoked pipeline signature.
        /// </summary>
        IPipelineMetadataBuilder InvokesPipeline<TRequest, TResponse>();

        /// <summary>
        /// Links a boundary to an invoked flow signature.
        /// </summary>
        IPipelineMetadataBuilder InvokesFlow<TRequest, TResponse>();

        /// <summary>
        /// Describes an external system crossed by a boundary.
        /// </summary>
        IPipelineMetadataBuilder External(string externalSystem);
    }
}
