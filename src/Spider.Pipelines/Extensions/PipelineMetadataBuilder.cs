namespace Spider.Pipelines.Extensions
{
    internal sealed class PipelineMetadataBuilder : IPipelineMetadataBuilder
    {
        public static readonly IPipelineMetadataBuilder Instance = new PipelineMetadataBuilder();

        private PipelineMetadataBuilder()
        {
        }

        public IPipelineMetadataBuilder Named(string name) => this;

        public IPipelineMetadataBuilder Describe(string description) => this;

        public IPipelineMetadataBuilder Tags(params string[] tags) => this;

        public IPipelineMetadataBuilder Metadata(string key, string value) => this;

        public IPipelineMetadataBuilder Purpose(string purpose) => this;

        public IPipelineMetadataBuilder Trigger(string trigger) => this;

        public IPipelineMetadataBuilder Wraps(string service) => this;

        public IPipelineMetadataBuilder Wraps<TService>() => this;

        public IPipelineMetadataBuilder Input<TInput>() => this;

        public IPipelineMetadataBuilder Output<TOutput>() => this;

        public IPipelineMetadataBuilder Policies(params string[] policies) => this;

        public IPipelineMetadataBuilder FailureBehavior(string behavior) => this;

        public IPipelineMetadataBuilder RelatedFlow(string flow) => this;

        public IPipelineMetadataBuilder RelatedFlow<TRequest, TResponse>() => this;

        public IPipelineMetadataBuilder Module(string module) => this;

        public IPipelineMetadataBuilder BoundaryType(string boundaryType) => this;

        public IPipelineMetadataBuilder EntryPoint(string entryPoint) => this;

        public IPipelineMetadataBuilder Protocol(string protocol) => this;

        public IPipelineMetadataBuilder Operation(string operation) => this;

        public IPipelineMetadataBuilder Contract<TRequest, TResponse>() => this;

        public IPipelineMetadataBuilder Security(params string[] security) => this;

        public IPipelineMetadataBuilder Sla(string sla) => this;

        public IPipelineMetadataBuilder Timeout(string timeout) => this;

        public IPipelineMetadataBuilder Observability(params string[] observability) => this;

        public IPipelineMetadataBuilder InvokesPipeline<TRequest, TResponse>() => this;

        public IPipelineMetadataBuilder InvokesFlow<TRequest, TResponse>() => this;

        public IPipelineMetadataBuilder External(string externalSystem) => this;
    }
}
