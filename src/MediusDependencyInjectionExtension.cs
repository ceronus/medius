using Microsoft.Extensions.DependencyInjection;

namespace Medius
{
    /// <summary>
    /// Provides extension methods for registering Medius services with the dependency injection container.
    /// </summary>
    public static class MediusDependencyInjectionExtension
    {
        /// <summary>
        /// Adds and configures the core Medius infrastructure within the service collection.
        /// This registers all discovered handler types and the <see cref="IMedius"/> dispatcher as a singleton.
        /// </summary>
        /// <param name="services">The service collection to which Medius services will be added.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection AddMedius(this IServiceCollection services) => Medius.CreateInstance(services);
    }
}