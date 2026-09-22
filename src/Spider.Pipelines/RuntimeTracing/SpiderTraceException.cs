namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Contains a serializable summary of an exception observed during runtime tracing.
    /// </summary>
    public sealed class SpiderTraceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderTraceException"/> class.
        /// </summary>
        /// <param name="type">The exception type name.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="stackTrace">The exception stack trace.</param>
        public SpiderTraceException(string type, string message, string stackTrace)
        {
            Type = type;
            Message = message;
            StackTrace = stackTrace;
        }

        /// <summary>
        /// Gets the exception type name.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the exception stack trace.
        /// </summary>
        public string StackTrace { get; }

        /// <summary>
        /// Creates a runtime trace exception summary from an exception.
        /// </summary>
        /// <param name="exception">The exception to summarize.</param>
        /// <returns>The exception summary.</returns>
        public static SpiderTraceException FromException(Exception exception)
        {
            if (exception == null)
                return null;

            return new SpiderTraceException(
                exception.GetType().FullName,
                exception.Message,
                exception.StackTrace);
        }
    }
}
