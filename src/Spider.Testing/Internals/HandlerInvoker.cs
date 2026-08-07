using System.Reflection;

namespace Spider.Testing.Internals
{
    /// <summary>
    /// Invokes discovered handler methods.
    /// </summary>
    internal static class HandlerInvoker
    {
        /// <summary>
        /// Invokes a discovered handler method.
        /// </summary>
        /// <param name="handler">The handler descriptor.</param>
        /// <param name="service">The handler service instance.</param>
        /// <param name="request">The request object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The handler result, or <c>null</c> for request-only handlers.</returns>
        public static async Task<object> InvokeAsync(
            HandlerDescriptor handler,
            object service,
            object request,
            CancellationToken cancellationToken)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var arguments = handler.AcceptsCancellationToken
                ? new[] { request, cancellationToken }
                : new[] { request };

            object result;
            try
            {
                result = handler.Method.Invoke(service, arguments);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }

            return await UnwrapAsync(result);
        }

        private static async Task<object> UnwrapAsync(object result)
        {
            if (result == null)
                return null;

            if (result is Task task)
            {
                await task;
                return ReadTaskResult(task);
            }

            if (result is ValueTask valueTask)
            {
                await valueTask;
                return null;
            }

            var resultType = result.GetType();
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var valueTaskAsTask = (Task)resultType.GetMethod("AsTask").Invoke(result, Array.Empty<object>());
                await valueTaskAsTask;
                return ReadTaskResult(valueTaskAsTask);
            }

            return result;
        }

        private static object ReadTaskResult(Task task)
        {
            var taskType = task.GetType();
            if (!taskType.IsGenericType)
                return null;

            return taskType.GetProperty("Result").GetValue(task);
        }
    }
}
