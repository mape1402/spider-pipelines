namespace Spider.Pipelines.Boundaries.Internals
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Resolves typed execution boundary implementations from dependency injection.
    /// </summary>
    internal static class ExecutionBoundaryTypeResolver
    {
        /// <summary>
        /// Resolves a request-only boundary type from dependency injection.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="serviceProvider">The service provider used to resolve the boundary.</param>
        /// <param name="boundaryType">The boundary implementation type to resolve.</param>
        /// <returns>The resolved request-only boundary.</returns>
        public static IBoundary<TRequest> Resolve<TRequest>(IServiceProvider serviceProvider, Type boundaryType)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var closedBoundaryType = CloseBoundaryType(boundaryType, typeof(TRequest));

            if (!typeof(IBoundary<TRequest>).IsAssignableFrom(closedBoundaryType))
                throw new InvalidOperationException($"Boundary type '{closedBoundaryType.FullName}' must implement IBoundary<{typeof(TRequest).Name}>.");

            return (IBoundary<TRequest>)serviceProvider.GetRequiredService(closedBoundaryType);
        }

        /// <summary>
        /// Resolves a request/response boundary type from dependency injection.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="serviceProvider">The service provider used to resolve the boundary.</param>
        /// <param name="boundaryType">The boundary implementation type to resolve.</param>
        /// <returns>The resolved request/response boundary.</returns>
        public static IBoundary<TRequest, TResponse> Resolve<TRequest, TResponse>(IServiceProvider serviceProvider, Type boundaryType)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var closedBoundaryType = CloseBoundaryType(boundaryType, typeof(TRequest), typeof(TResponse));

            if (!typeof(IBoundary<TRequest, TResponse>).IsAssignableFrom(closedBoundaryType))
                throw new InvalidOperationException($"Boundary type '{closedBoundaryType.FullName}' must implement IBoundary<{typeof(TRequest).Name}, {typeof(TResponse).Name}>.");

            return (IBoundary<TRequest, TResponse>)serviceProvider.GetRequiredService(closedBoundaryType);
        }

        /// <summary>
        /// Closes an open generic boundary type with the supplied pipeline type arguments.
        /// </summary>
        /// <param name="boundaryType">The boundary implementation type to close.</param>
        /// <param name="typeArguments">The pipeline type arguments used to close the boundary type.</param>
        /// <returns>The closed boundary implementation type.</returns>
        private static Type CloseBoundaryType(Type boundaryType, params Type[] typeArguments)
        {
            if (boundaryType == null)
                throw new ArgumentNullException(nameof(boundaryType));

            if (!boundaryType.ContainsGenericParameters)
                return boundaryType;

            if (!boundaryType.IsGenericTypeDefinition)
                throw new InvalidOperationException($"Boundary type '{boundaryType.FullName}' must be a generic type definition.");

            var genericArguments = boundaryType.GetGenericArguments();

            if (genericArguments.Length != typeArguments.Length)
                throw new InvalidOperationException($"Boundary type '{boundaryType.FullName}' expects {genericArguments.Length} generic argument(s), but the pipeline provides {typeArguments.Length}.");

            return boundaryType.MakeGenericType(typeArguments);
        }
    }
}
