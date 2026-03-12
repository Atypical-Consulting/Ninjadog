namespace Ninjadog.Engine.Core.Abstractions;

/// <summary>
/// Defines the interface for a domain event dispatcher.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches the specified domain event to its respective handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <returns>A task that represents the asynchronous dispatch operation.</returns>
    Task Dispatch(IDomainEvent domainEvent);

    /// <summary>
    /// Registers an external handler for the specified domain event.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <typeparam name="T">The type of the domain event to register the handler for.</typeparam>
    void RegisterHandler<T>(Func<T, Task> handler)
        where T : IDomainEvent;
}
