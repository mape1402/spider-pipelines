using System.Reflection;
using Spider.Pipelines.Boundaries;

namespace Spider.Testing.Internals
{
    /// <summary>
    /// Discovers Spider testing handlers and boundaries from assemblies.
    /// </summary>
    internal static class SpiderTestingDiscovery
    {
        /// <summary>
        /// Discovers a registry from the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>The discovered testing registry.</returns>
        public static SpiderTestingRegistry Discover(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            var types = assemblies
                .SelectMany(GetLoadableTypes)
                .Where(type => type is { IsClass: true, IsAbstract: false })
                .ToArray();

            var boundaries = types
                .Where(type => typeof(IPipelineExecutionBoundary).IsAssignableFrom(type))
                .Select(type => new BoundaryDescriptor(type))
                .OrderBy(descriptor => descriptor.Name, StringComparer.Ordinal)
                .ToArray();

            var handlers = types
                .SelectMany(CreateHandlerDescriptors)
                .ToArray();

            return new SpiderTestingRegistry(boundaries, handlers);
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
        }

        private static IEnumerable<HandlerDescriptor> CreateHandlerDescriptors(Type serviceType)
            => serviceType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsHandlerMethod)
                .Select(method =>
                {
                    var requestType = method.GetParameters()[0].ParameterType;
                    var responseType = GetResponseType(method.ReturnType);
                    return new HandlerDescriptor(serviceType, method, requestType, responseType);
                });

        private static bool IsHandlerMethod(MethodInfo method)
        {
            if (method == null)
                return false;

            if (method.IsSpecialName)
                return false;

            if (!string.Equals(method.Name, "Handle", StringComparison.Ordinal)
                && !string.Equals(method.Name, "HandleAsync", StringComparison.Ordinal))
                return false;

            var parameters = method.GetParameters();
            if (parameters.Length is < 1 or > 2)
                return false;

            if (parameters[0].ParameterType == typeof(CancellationToken))
                return false;

            if (parameters.Length == 2 && parameters[1].ParameterType != typeof(CancellationToken))
                return false;

            return method.ReturnType == typeof(void)
                   || method.ReturnType == typeof(Task)
                   || method.ReturnType == typeof(ValueTask)
                   || IsGenericTask(method.ReturnType)
                   || IsGenericValueTask(method.ReturnType)
                   || method.ReturnType != typeof(void);
        }

        private static Type GetResponseType(Type returnType)
        {
            if (returnType == typeof(void)
                || returnType == typeof(Task)
                || returnType == typeof(ValueTask))
                return null;

            if (IsGenericTask(returnType) || IsGenericValueTask(returnType))
                return returnType.GetGenericArguments()[0];

            return returnType;
        }

        private static bool IsGenericTask(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);

        private static bool IsGenericValueTask(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);
    }
}
