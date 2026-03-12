namespace Ninjadog.CLI.Infrastructure;

/// <summary>
/// Resolves services of specific types from the service provider.
/// </summary>
public sealed class TypeResolver
    : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeResolver"/> class.
    /// </summary>
    /// <param name="provider">The service provider used to resolve services.</param>
    public TypeResolver(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    /// <summary>
    /// Resolves and returns the service of the specified type.
    /// </summary>
    /// <param name="type">The type of the service to resolve.</param>
    /// <returns>The resolved service object or null if the service does not exist.</returns>
    public object? Resolve(Type? type)
    {
        return type == null
            ? null
            : _provider.GetService(type);
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="TypeResolver"/>.
    /// </summary>
    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
