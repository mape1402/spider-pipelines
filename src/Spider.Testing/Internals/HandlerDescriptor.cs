using System.Reflection;

namespace Spider.Testing.Internals
{
    /// <summary>
    /// Describes a discovered request handler.
    /// </summary>
    internal sealed class HandlerDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerDescriptor"/> class.
        /// </summary>
        /// <param name="serviceType">The service type that owns the handler method.</param>
        /// <param name="method">The handler method.</param>
        /// <param name="requestType">The request type accepted by the handler.</param>
        /// <param name="responseType">The response type returned by the handler, or <c>null</c> for request-only handlers.</param>
        public HandlerDescriptor(Type serviceType, MethodInfo method, Type requestType, Type responseType)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
            ResponseType = responseType;
        }

        /// <summary>
        /// Gets the service type that owns the handler method.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the handler method.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the request type accepted by the handler.
        /// </summary>
        public Type RequestType { get; }

        /// <summary>
        /// Gets the response type returned by the handler, or <c>null</c> for request-only handlers.
        /// </summary>
        public Type ResponseType { get; }

        /// <summary>
        /// Gets a value indicating whether the handler returns a response.
        /// </summary>
        public bool HasResponse => ResponseType != null;

        /// <summary>
        /// Gets a value indicating whether the handler accepts a cancellation token.
        /// </summary>
        public bool AcceptsCancellationToken => Method.GetParameters().Length == 2;
    }
}
