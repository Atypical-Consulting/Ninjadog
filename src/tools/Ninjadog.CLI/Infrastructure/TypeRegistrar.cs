using Microsoft.Extensions.DependencyInjection;

namespace Ninjadog.CLI.Infrastructure;

/// <summary>
/// Provides a mechanism for registering types and their implementations with a service collection.
/// </summary>
/// <param name="builder">The service collection to which types will be registered.</param>
public sealed class TypeRegistrar(IServiceCollection builder)
    : ITypeRegistrar
{
    /// <summary>
    /// Builds and returns an instance of <see cref="ITypeResolver"/> to resolve types from the service collection.
    /// </summary>
    /// <returns>An instance of <see cref="ITypeResolver"/>.</returns>
    public ITypeResolver Build()
    {
        return new TypeResolver(builder.BuildServiceProvider());
    }

    /// <summary>
    /// Registers a service type with its implementation in the service collection as a singleton.
    /// </summary>
    /// <param name="service">The service type to register.</param>
    /// <param name="implementation">The implementation type of the service.</param>
    public void Register(Type service, Type implementation)
    {
        builder.AddSingleton(service, implementation);
    }

    /// <summary>
    /// Registers a specific instance of a service in the service collection as a singleton.
    /// </summary>
    /// <param name="service">The service type to register.</param>
    /// <param name="implementation">The instance of the service to register.</param>
    public void RegisterInstance(Type service, object implementation)
    {
        builder.AddSingleton(service, implementation);
    }

    /// <summary>
    /// Registers a service with a factory function that produces its implementation, added as a singleton.
    /// </summary>
    /// <param name="service">The service type to register.</param>
    /// <param name="factory">The factory function to create the service instance.</param>
    public void RegisterLazy(Type service, Func<object> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        builder.AddSingleton(service, _ => factory());
    }
}
