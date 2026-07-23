namespace Spider.Pipelines.Extensions
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Provides extension methods for working with pipeline context objects, including state transitions, result handling, and safe casting.
    /// </summary>
    public static class ContextExtensions
    {
        /// <summary>
        /// Cancels the ongoing operation associated with the context.
        /// </summary>
        /// <param name="context">The read-only context instance.</param>
        public static void CancelOperation(this IReadOnlyContext context)
        {
            var cancellable = context.AsCancellable();
            cancellable.CancelOperation();
        }

        /// <summary>
        /// Cancels the ongoing operation associated with the context.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="context">The read-only context instance.</param>
        public static void CancelOperation<TRequest>(this IReadOnlyContext<TRequest> context)
        {
            var cancellable = context.AsCancellable();
            cancellable.CancelOperation();
        }

        /// <summary>
        /// Cancels the ongoing operation associated with the context.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="context">The read-only context instance.</param>
        public static void CancelOperation<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
        {
            var cancellable = context.AsCancellable();
            cancellable.CancelOperation();
        }

        /// <summary>
        /// Sets the pipeline state to <see cref="PipelineState.OnPreProcess"/> for preprocessing.
        /// </summary>
        public static void OnPreProcess<TRequest>(this IReadOnlyContext<TRequest> context)
        {
            var settable = context.AsSettable();
            settable.SetPipelineState(PipelineState.OnPreProcess);
        }

        /// <summary>
        /// Sets the pipeline state to <see cref="PipelineState.OnPreProcess"/> for preprocessing.
        /// </summary>
        public static void OnPreProcess<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
        {
            var settable = context.AsSettable();
            settable.SetPipelineState(PipelineState.OnPreProcess);
        }

        /// <summary>
        /// Sets the pipeline state to <see cref="PipelineState.OnTargeting"/> for targeting/override.
        /// </summary>
        public static void OnTargeting<TRequest>(this IReadOnlyContext<TRequest> context)
        {
            var settable = context.AsSettable();
            settable.SetPipelineState(PipelineState.OnTargeting);
        }

        /// <summary>
        /// Sets the pipeline state to <see cref="PipelineState.OnTargeting"/> for targeting/override.
        /// </summary>
        public static void OnTargeting<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
        {
            var settable = context.AsSettable();
            settable.SetPipelineState(PipelineState.OnTargeting);
        }

        /// <summary>
        /// Sets the pipeline state to <see cref="PipelineState.OnPostProcess"/> for postprocessing.
        /// </summary>
        public static void OnPostProcess<TRequest>(this IReadOnlyContext<TRequest> context)
        {
            var settable = context.AsSettable();
            settable.SetPipelineState(PipelineState.OnPostProcess);
        }

        /// <summary>
        /// Sets the pipeline state to <see cref="PipelineState.OnPostProcess"/> for postprocessing.
        /// </summary>
        public static void OnPostProcess<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
        {
            var settable = context.AsSettable();
            settable.SetPipelineState(PipelineState.OnPostProcess);
        }

        /// <summary>
        /// Safely casts a read-only context to a settable context, or throws an informative exception if not possible.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="context">The read-only context instance.</param>
        /// <returns>The settable context instance.</returns>
        /// <exception cref="InvalidCastException">Thrown if the context does not implement <see cref="ISettableContext{TRequest}"/>.</exception>
        public static ISettableContext<TRequest> AsSettable<TRequest>(this IReadOnlyContext<TRequest> context)
            => context is ISettableContext<TRequest> settable
                ? settable
                : throw new InvalidCastException($"Context of type '{context?.GetType().FullName}' does not implement ISettableContext<{typeof(TRequest).Name}>.");

        /// <summary>
        /// Safely casts a read-only context to a settable context, or throws an informative exception if not possible.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="context">The read-only context instance.</param>
        /// <returns>The settable context instance.</returns>
        /// <exception cref="InvalidCastException">Thrown if the context does not implement <see cref="ISettableContext{TRequest, TResponse}"/>.</exception>
        public static ISettableContext<TRequest, TResponse> AsSettable<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
            => context is ISettableContext<TRequest, TResponse> settable
                ? settable
                : throw new InvalidCastException($"Context of type '{context?.GetType().FullName}' does not implement ISettableContext<{typeof(TRequest).Name}, {typeof(TResponse).Name}>.");

        /// <summary>
        /// Safely casts a read-only context to a cancellable context, or throws an informative exception if not possible.
        /// </summary>
        /// <param name="context">The read-only context instance.</param>
        /// <returns>The cancellable context instance.</returns>
        /// <exception cref="InvalidCastException">Thrown if the context does not implement <see cref="ICancellableContext"/>.</exception>
        public static ICancellableContext AsCancellable(this IReadOnlyContext context)
            => context is ICancellableContext cancellable
                ? cancellable
                : throw new InvalidCastException($"Context of type '{context?.GetType().FullName}' does not implement ICancellableContext.");

        /// <summary>
        /// Safely casts a read-only context to a cancellable context, or throws an informative exception if not possible.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="context">The read-only context instance.</param>
        /// <returns>The cancellable context instance.</returns>
        /// <exception cref="InvalidCastException">Thrown if the context does not implement <see cref="ICancellableContext"/>.</exception>
        public static ICancellableContext AsCancellable<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
            => context is ICancellableContext cancellable
                ? cancellable
                : throw new InvalidCastException($"Context of type '{context?.GetType().FullName}' does not implement ICancellableContext.");

        /// <summary>
        /// Safely casts a read-only context to a cancellable context, or throws an informative exception if not possible.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="context">The read-only context instance.</param>
        /// <returns>The cancellable context instance.</returns>
        /// <exception cref="InvalidCastException">Thrown if the context does not implement <see cref="ICancellableContext"/>.</exception>
        public static ICancellableContext AsCancellable<TRequest>(this IReadOnlyContext<TRequest> context)
            => context is ICancellableContext cancellable
                ? cancellable
                : throw new InvalidCastException($"Context of type '{context?.GetType().FullName}' does not implement ICancellableContext.");

        /// <summary>
        /// Sets the result state to <see cref="ResultState.Success"/>.
        /// </summary>
        public static void Success<TRequest>(this ISettableContext<TRequest> context)
            => context.SetResultState(ResultState.Success);

        /// <summary>
        /// Sets the result state to <see cref="ResultState.Success"/> and assigns the response.
        /// </summary>
        public static void Success<TRequest, TResponse>(this ISettableContext<TRequest, TResponse> context, TResponse response)
        {
            context.SetResultState(ResultState.Success);
            context.SetResponse(response);
        }

        /// <summary>
        /// Sets the result state to <see cref="ResultState.Failure"/> and assigns the exception.
        /// </summary>
        public static void Failure<TRequest>(this ISettableContext<TRequest> context, Exception exception)
        {
            context.SetResultState(ResultState.Failure);
            context.SetException(exception);
        }

        /// <summary>
        /// Sets the result state to <see cref="ResultState.Failure"/> and assigns the exception.
        /// </summary>
        public static void Failure<TRequest, TResponse>(this ISettableContext<TRequest, TResponse> context, Exception exception)
        {
            context.SetResultState(ResultState.Failure);
            context.SetException(exception);
        }

        /// <summary>
        /// Sets the result state to <see cref="ResultState.Failure"/>.
        /// </summary>
        public static void Failure<TRequest>(this ISettableContext<TRequest> context)
            => context.SetResultState(ResultState.Failure);

        /// <summary>
        /// Sets the result state to <see cref="ResultState.Failure"/>.
        /// </summary>
        public static void Failure<TRequest, TResponse>(this ISettableContext<TRequest, TResponse> context)
            => context.SetResultState(ResultState.Failure);

        /// <summary>
        /// Sets the result state to <see cref="ResultState.Cancelled"/>.
        /// </summary>
        public static void Cancelled<TRequest>(this ISettableContext<TRequest> context)
            => context.SetResultState(ResultState.Cancelled);

        /// <summary>
        /// Sets the result state to <see cref="ResultState.Cancelled"/>.
        /// </summary>
        public static void Cancelled<TRequest, TResponse>(this ISettableContext<TRequest, TResponse> context)
            => context.SetResultState(ResultState.Cancelled);

        /// <summary>
        /// Determines if the context result state is <see cref="ResultState.Success"/>.
        /// </summary>
        public static bool IsSuccess<TRequest>(this IReadOnlyContext<TRequest> context)
            => context.ResultState == ResultState.Success;

        /// <summary>
        /// Determines if the context result state is <see cref="ResultState.Success"/>.
        /// </summary>
        public static bool IsSuccess<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
            => context.ResultState == ResultState.Success;

        /// <summary>
        /// Determines if the context result state is <see cref="ResultState.Failure"/>.
        /// </summary>
        public static bool IsFailure<TRequest>(this IReadOnlyContext<TRequest> context)
            => context.ResultState == ResultState.Failure;

        /// <summary>
        /// Determines if the context result state is <see cref="ResultState.Failure"/>.
        /// </summary>
        public static bool IsFailure<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
            => context.ResultState == ResultState.Failure;

        /// <summary>
        /// Determines if the context result state is <see cref="ResultState.Pending"/>.
        /// </summary>
        public static bool IsPending<TRequest>(this IReadOnlyContext<TRequest> context)
            => context.ResultState == ResultState.Pending;

        /// <summary>
        /// Determines if the context result state is <see cref="ResultState.Pending"/>.
        /// </summary>
        public static bool IsPending<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
            => context.ResultState == ResultState.Pending;

        /// <summary>
        /// Determines if the context result state is <see cref="ResultState.Cancelled"/>.
        /// </summary>
        public static bool IsCancelledResult<TRequest>(this IReadOnlyContext<TRequest> context)
            => context.ResultState == ResultState.Cancelled;

        /// <summary>
        /// Determines if the context result state is <see cref="ResultState.Cancelled"/>.
        /// </summary>
        public static bool IsCancelledResult<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
            => context.ResultState == ResultState.Cancelled;

        /// <summary>
        /// Determines if the context has been cancelled.
        /// </summary>
        public static bool IsCancelled<TRequest>(this IReadOnlyContext<TRequest> context)
            => context.Cancelled || context.CancellationToken.IsCancellationRequested;

        /// <summary>
        /// Determines if the context has been cancelled.
        /// </summary>
        public static bool IsCancelled<TRequest, TResponse>(this IReadOnlyContext<TRequest, TResponse> context)
            => context.Cancelled || context.CancellationToken.IsCancellationRequested;
    }
}
