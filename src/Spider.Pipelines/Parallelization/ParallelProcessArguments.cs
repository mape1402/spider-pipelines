namespace Spider.Pipelines.Parallelization
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Represents the arguments passed to a parallel processing step in the pipeline.
    /// </summary>
    public class ParallelProcessArguments
    {
        /// <summary>
        /// Gets or sets a value indicating whether the parallel execution should stop immediately 
        /// if any of the operations fail or throw an exception.
        /// </summary>
        public bool StopOnFirstFailure { get; set; } = false;

        /// <summary>
        /// Gets or sets an optional shared context object that can be used to accumulate results 
        /// or share state between parallel tasks.
        /// </summary>
        public ConcurrentDictionary<string, object> SharedContext { get; set; }
    }
}