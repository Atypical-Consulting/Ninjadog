using Microsoft.Extensions.DependencyInjection;

namespace Ninjadog.Engine.Core.DomainEvents;

/// <summary>
/// Provides an implementation for dispatching domain events.
/// </summary>
/// <param name="serviceProvider">The service provider used for resolving event handlers.</param>
/// <remarks>
/// Initializes a new instance of the <see cref="DomainEventDispatcher"/> class.
/// </remarks>
public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];

    /// <inheritdoc />
    public async Task Dispatch(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();

        // First, invoke statically resolved handlers
        var handlerType = typeof(IDomainEventProcessor<>).MakeGenericType(eventType);
#pragma warning disable IDE0340
        var resolvedHandlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in resolvedHandlers)
        {
            var method = handlerType.GetMethod(nameof(IDomainEventProcessor<IDomainEvent>.HandleAsync));
#pragma warning restore IDE0340
            if (method is not null)
            {
                await ((Task)method.Invoke(handler, [domainEvent])!).ConfigureAwait(false);
            }
        }

        // Then, invoke dynamically registered handlers
        if (_handlers.TryGetValue(eventType, out var eventHandlers))
        {
            foreach (var handler in eventHandlers)
            {
                var handlerTask = (Task)handler.DynamicInvoke(domainEvent)!;
                await handlerTask.ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public void RegisterHandler<T>(Func<T, Task> handler)
        where T : IDomainEvent
    {
        var eventType = typeof(T);
        if (!_handlers.TryGetValue(eventType, out var value))
        {
            value = [];
            _handlers[eventType] = value;
        }

        value.Add(handler);
    }
}
