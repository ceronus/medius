using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Medius
{
    public class Medius : IMedius
    {
        private readonly IList<Type> _handlerTypes;
        private readonly Dictionary<MediusRouteKey, MediusHandlerInfo> _routes;

        internal IServiceProvider ServiceProvider { get; private set; }

        internal Medius(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            _handlerTypes = GetHandlers();
            _routes = GetRoutes(_handlerTypes);
            foreach (Type type in _handlerTypes)
            {
                services.AddSingleton(type);
            }
        }

        public static IServiceCollection CreateInstance(IServiceCollection services)
        {
            Medius medius = new(services);
            services.AddSingleton<IMedius, Medius>(serviceProvider =>
            {
                medius.ServiceProvider = serviceProvider;
                return medius;
            });
            return services;
        }

        public Task<TActionResult> InvokeAsync<TActionResult>(IMediusAction<TActionResult> action, CancellationToken cancellationToken = default)
            => InvokeAsync((IMediusOperation<TActionResult>)action, cancellationToken);

        public Task InvokeAsync(IMediusCommand command, CancellationToken cancellationToken = default)
            => InvokeAsync((IMediusOperation<MediusUndefinedType>)command, cancellationToken);

        public Task<TQueryResult> InvokeAsync<TQueryResult>(IMediusQuery<TQueryResult> query, CancellationToken cancellationToken = default)
            => InvokeAsync((IMediusOperation<TQueryResult>)query, cancellationToken);

        public async Task<TOperationResult> InvokeAsync<TOperationResult>(IMediusOperation<TOperationResult> operation, CancellationToken cancellationToken = default)
        {
            MediusRouteKey key = new(operation.GetType(), typeof(TOperationResult));

            if (_routes.Keys.Contains(key))
            {
                MediusHandlerInfo handler = _routes[key];
                object instance = ServiceProvider.GetRequiredService(handler.Type);
                object[] parameters = new object[2] { operation, cancellationToken };
                Task<TOperationResult> task = (Task<TOperationResult>)handler.MethodInfo.Invoke(instance, parameters);
                return await task.ConfigureAwait(false);
            }

            throw new InvalidOperationException("No matching handler found.");
        }

        private static IList<Type> GetHandlers()
        {
            List<Type> results = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                results.AddRange(assembly.GetTypes().Where(type => typeof(IMediusHandler).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)?.ToList());
            }

            return results;
        }

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

        private static MediusRouteKey CreateRouteKey(Type mediusHandler)
        {
            Type[] generics = mediusHandler.GenericTypeArguments;
            if (generics.Length < 1 || generics.Length > 2) throw new InvalidOperationException($"Invalid handler. Unsupported number of generics in handler '{handlerType.Name}'.");
            Type operation = generics[0];
            Type result = generics.Length == 2 ? generics[1] : typeof(MediusUndefinedType);
            return new(operation, result);
        }

        #region Private Structures
        private readonly struct MediusRouteKey : IEquatable<MediusRouteKey>
        {
            public Type OperationType { get; }
            public Type ResultType { get; }

            public MediusRouteKey(Type operation, Type result)
            {
                this.OperationType = operation;
                this.ResultType = result;
            }

            public override bool Equals(object obj)
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