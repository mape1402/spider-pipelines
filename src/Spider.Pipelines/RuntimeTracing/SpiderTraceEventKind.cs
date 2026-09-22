namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Describes the semantic kind of a runtime trace event.
    /// </summary>
    public enum SpiderTraceEventKind
    {
        /// <summary>
        /// A trace started.
        /// </summary>
        TraceStarted,

        /// <summary>
        /// A trace completed successfully.
        /// </summary>
        TraceCompleted,

        /// <summary>
        /// A trace faulted.
        /// </summary>
        TraceFaulted,

        /// <summary>
        /// A trace was cancelled.
        /// </summary>
        TraceCancelled,

        /// <summary>
        /// A pipeline started.
        /// </summary>
        PipelineStarted,

        /// <summary>
        /// A pipeline completed successfully.
        /// </summary>
        PipelineCompleted,

        /// <summary>
        /// A pipeline faulted.
        /// </summary>
        PipelineFaulted,

        /// <summary>
        /// A pipeline was cancelled.
        /// </summary>
        PipelineCancelled,

        /// <summary>
        /// A pipeline stage started.
        /// </summary>
        PipelineStageStarted,

        /// <summary>
        /// A pipeline stage completed successfully.
        /// </summary>
        PipelineStageCompleted,

        /// <summary>
        /// A pipeline stage faulted.
        /// </summary>
        PipelineStageFaulted,

        /// <summary>
        /// A boundary begin operation started.
        /// </summary>
        BoundaryBeginStarted,

        /// <summary>
        /// A boundary begin operation completed.
        /// </summary>
        BoundaryBeginCompleted,

        /// <summary>
        /// A boundary complete operation started.
        /// </summary>
        BoundaryCompleteStarted,

        /// <summary>
        /// A boundary complete operation completed.
        /// </summary>
        BoundaryCompleteCompleted,

        /// <summary>
        /// A boundary fault operation started.
        /// </summary>
        BoundaryFaultStarted,

        /// <summary>
        /// A boundary fault operation completed.
        /// </summary>
        BoundaryFaultCompleted,

        /// <summary>
        /// A boundary cancel operation started.
        /// </summary>
        BoundaryCancelStarted,

        /// <summary>
        /// A boundary cancel operation completed.
        /// </summary>
        BoundaryCancelCompleted,

        /// <summary>
        /// A flow started.
        /// </summary>
        FlowStarted,

        /// <summary>
        /// A flow completed successfully.
        /// </summary>
        FlowCompleted,

        /// <summary>
        /// A flow faulted.
        /// </summary>
        FlowFaulted,

        /// <summary>
        /// A flow was cancelled.
        /// </summary>
        FlowCancelled,

        /// <summary>
        /// A flow step started.
        /// </summary>
        FlowStepStarted,

        /// <summary>
        /// A flow step completed successfully.
        /// </summary>
        FlowStepCompleted,

        /// <summary>
        /// A flow step faulted.
        /// </summary>
        FlowStepFaulted,

        /// <summary>
        /// A flow condition was evaluated.
        /// </summary>
        FlowConditionEvaluated,

        /// <summary>
        /// A flow branch route was selected.
        /// </summary>
        FlowBranchSelected,

        /// <summary>
        /// A flow branch route was skipped.
        /// </summary>
        FlowBranchSkipped,

        /// <summary>
        /// A trace event was dropped.
        /// </summary>
        EventDropped
    }
}
