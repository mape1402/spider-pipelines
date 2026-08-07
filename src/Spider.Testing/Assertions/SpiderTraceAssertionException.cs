namespace Spider.Testing.Assertions
{
    /// <summary>
    /// Represents an assertion failure raised by Spider testing trace assertions.
    /// </summary>
    public sealed class SpiderTraceAssertionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderTraceAssertionException"/> class.
        /// </summary>
        /// <param name="message">The assertion failure message.</param>
        public SpiderTraceAssertionException(string message)
            : base(message)
        {
        }
    }
}
