using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Medius
{
    /// <summary>
    /// Central dispatcher that locates and executes Medius handlers.
    /// Discovers concrete types implementing <see cref="IMediusHandler"/>, registers them with DI,
    /// and routes incoming operations to their corresponding handlers within a created scope.
    /// </summary>
    /// <remarks>
    /// <para>Thread-safety:</para>
    /// <para><see cref="Medius"/> is registered as a singleton and is safe for concurrent use. Each invocation
    /// creates its own DI scope, so scoped dependencies (e.g., DbContext) are isolated per operation.</para>
    /// </remarks>
    public class Medius : IMedius
    {
        private readonly IList<Type> _handlerTypes;
        private readonly Dictionary<MediusRouteKey, MediusHandlerInfo> _routes;

        private IServiceScopeFactory? _scopeFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="Medius"/> and registers discovered handler types as transient services.
        /// </summary>
        /// <param name="services">The current service collection.</param>
        internal Medius(IServiceCollection services)
        {
            _handlerTypes = GetHandlers();
            _routes = GetRoutes(_handlerTypes);
            foreach (Type type in _handlerTypes)
            {
                services.AddTransient(type);
            }
        }

        /// <summary>
        /// Captures the application <see cref="IServiceScopeFactory"/> from the built service provider.
        /// Must be called after the container has been fully built.
        /// </summary>
        /// <param name="serviceProvider">The application's root service provider.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown by downstream calls if <see cref="_scopeFactory"/> is not initialized.
        /// </exception>
        internal void Initialize(IServiceProvider serviceProvider)
            => _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();


        /// <summary>
        /// Adds and configures Medius within the service collection:
        /// discovers and registers handler types as transient services and registers <see cref="IMedius"/> as a singleton.
        /// </summary>
        /// <param name="services">The current service collection.</param>
        /// <returns>The same service collection for chaining.</returns>
        public static IServiceCollection CreateInstance(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            Medius medius = new(services);
            services.AddSingleton<IMedius, Medius>(serviceProvider =>
            {
                medius.Initialize(serviceProvider);
                return medius;
            });
            return services;
        }

        /// <inheritdoc />
        public Task<TActionResult?> InvokeAsync<TActionResult>(IMediusAction<TActionResult> action, CancellationToken cancellationToken = default)
            => InvokeAsync((IMediusOperation<TActionResult>)action, cancellationToken);

        /// <inheritdoc />
        public Task InvokeAsync(IMediusCommand command, CancellationToken cancellationToken = default)
            => InvokeAsync((IMediusOperation<MediusUndefinedType>)command, cancellationToken);

        /// <inheritdoc />
        public Task<TQueryResult?> InvokeAsync<TQueryResult>(IMediusQuery<TQueryResult> query, CancellationToken cancellationToken = default)
            => InvokeAsync((IMediusOperation<TQueryResult>)query, cancellationToken);

        /// <summary>
        /// Executes the specified <see cref="IMediusOperation{TOperationResult}"/> by locating its handler
        /// and invoking it within a newly created DI scope.
        /// </summary>
        /// <typeparam name="TOperationResult">The expected result type of the operation.</typeparam>
        /// <param name="operation">The operation instance to execute.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task containing the operation result.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if Medius has not been initialized or if no matching handler is found,
        /// or if the handler method could not be invoked.
        /// </exception>
        public async Task<TOperationResult?> InvokeAsync<TOperationResult>(IMediusOperation<TOperationResult> operation, CancellationToken cancellationToken = default)
        {
            if (_scopeFactory is null) throw new InvalidOperationException("Medius not initialized.");
            
            MediusRouteKey key = new(operation.GetType(), typeof(TOperationResult));
            if (!_routes.TryGetValue(key, out MediusHandlerInfo handler)) throw new InvalidOperationException("No matching handler found.");

            Debug.Assert(_scopeFactory != null);
            using IServiceScope scope = _scopeFactory.CreateScope();
            object instance = scope.ServiceProvider.GetRequiredService(handler.Type);
            object[] parameters = new object[2] { operation, cancellationToken };
            Task<TOperationResult?>? task = handler.MethodInfo.Invoke(instance, parameters) as Task<TOperationResult?>;
            if (task == null) throw new InvalidOperationException("Unable to invoke handler.");
            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Scans loaded assemblies and returns all non-abstract classes implementing <see cref="IMediusHandler"/>.
        /// </summary>
        /// <remarks>
        /// Depending on the hosting model, not all assemblies may be loaded at scan time.
        /// If necessary, ensure assemblies containing handlers are loaded before calling this method.
        /// </remarks>
        private static IList<Type> GetHandlers()
        {
            List<Type> results = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                results.AddRange(assembly.GetTypes().Where(type => typeof(IMediusHandler).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract).ToList());
            }

            return results;
        }

        /// <summary>
        /// Builds the routing table that maps operation/result pairs to handler metadata.
        /// </summary>
        /// <param name="handlerTypes">The discovered handler types.</param>
        /// <returns>A dictionary keyed by operation/result to the corresponding handler info.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if multiple handlers map to the same operation/result pair.
        /// </exception>
        private static Dictionary<MediusRouteKey, MediusHandlerInfo> GetRoutes(IEnumerable<Type> handlerTypes)
        {
            Dictionary<MediusRouteKey, MediusHandlerInfo> routes = new();

            foreach (Type handlerType in handlerTypes)
            {
                Type rootHandlerType = GetRootHandlerType(handlerType);
                MediusRouteKey key = CreateRouteKey(rootHandlerType);
                if (routes.ContainsKey(key)) throw new InvalidOperationException($"Must use unique operations. Operation for '{handlerType.Name}' is a duplicate.");
                MediusHandlerInfo value = new(handlerType);
                routes.Add(key, value);
            }

            return routes;
        }

        /// <summary>
        /// Walks the inheritance chain to the topmost Medius handler type for a given handler.
        /// This normalizes routing for derived handlers that specialize base handler types.
        /// </summary>
        private static Type GetRootHandlerType(Type handlerType)
        {
            Type mediusHandler = handlerType;

            while (typeof(IMediusHandler).IsAssignableFrom(mediusHandler.BaseType))
            {
                // e.g. Object <-- [MediusFooHandler] <-- DerivedHandler <-- FurtherDerivedHandler(N)
                mediusHandler = mediusHandler.BaseType;
            }

            return mediusHandler;
        }

        /// <summary>
        /// Creates a unique route key from a handler's generic arguments.
        /// Uses <see cref="MediusUndefinedType"/> as the result type when the handler declares only one generic argument.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the handler type declares an unsupported number of generics.
        /// </exception>
        private static MediusRouteKey CreateRouteKey(Type mediusHandler)
        {
            Type[] generics = mediusHandler.GenericTypeArguments;
            if (generics.Length < 1 || generics.Length > 2) throw new InvalidOperationException($"Invalid handler. Unsupported number of generics in handler '{mediusHandler.Name}'.");
            Type operation = generics[0];
            Type result = generics.Length == 2 ? generics[1] : typeof(MediusUndefinedType);
            return new(operation, result);
        }

        #region Private Structures
        /// <summary>
        /// Represents a unique mapping key combining an operation type and its expected result type.
        /// Used internally to route requests to the correct handler.
        /// </summary>
        private readonly struct MediusRouteKey : IEquatable<MediusRouteKey>
        {
            public Type OperationType { get; }
            public Type ResultType { get; }

            public MediusRouteKey(Type operation, Type result)
            {
                this.OperationType = operation;
                this.ResultType = result;
            }

            public override bool Equals(object? obj)
            {
                if (obj is null || obj is not MediusRouteKey) return false;

                return this.Equals((MediusRouteKey)obj);
            }

            public bool Equals(MediusRouteKey other)
            {
                if (!other.OperationType.GUID.Equals(this.OperationType.GUID)) return false;
                if (!other.ResultType.GUID.Equals(this.ResultType.GUID)) return false;

                return true;
            }

            public static bool operator ==(MediusRouteKey left, MediusRouteKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(MediusRouteKey left, MediusRouteKey right)
            {
                return !(left == right);
            }

            public override int GetHashCode()
            {
                HashCode hash = new();
                hash.Add(OperationType.GUID);
                hash.Add(ResultType.GUID);
                return hash.ToHashCode();
            }
            public override string ToString() => $"{OperationType?.FullName ?? "Undefined"}<{ResultType?.FullName ?? "Undefined"}>";
        }

        /// <summary>
        /// Stores handler metadata: the concrete handler <see cref="Type"/> and the method used for invocation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlerType"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="handlerType"/> does not implement <see cref="IMediusHandler"/> or
        /// when the expected <c>HandleAsync</c> method cannot be found.
        /// </exception>
        private struct MediusHandlerInfo
        {
            public MethodInfo MethodInfo { get; set; }
            public Type Type { get; set; }

            public MediusHandlerInfo(Type handlerType)
            {
                if (handlerType == null) throw new ArgumentNullException(nameof(handlerType));

                if (!typeof(IMediusHandler).IsAssignableFrom(handlerType)) throw new ArgumentException($"Invalid type. Not of type {nameof(IMediusHandler)}", nameof(handlerType));

                string methodName = nameof(IMediusBaseHandler<object, object>.HandleAsync);
                MethodInfo methodInfo = handlerType.GetMethod(methodName) ?? throw new ArgumentException($"Invalid type. The method '{methodName}' does not exist in type.", nameof(handlerType));
                this.MethodInfo = methodInfo;
                this.Type = handlerType;
            }
        }
        #endregion
    }
}