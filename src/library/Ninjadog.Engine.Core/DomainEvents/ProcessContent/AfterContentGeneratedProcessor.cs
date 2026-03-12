namespace Ninjadog.Engine.Core.DomainEvents.ProcessContent;

/// <summary>
/// Handles events that occur after content is generated.
/// </summary>
public class AfterContentGeneratedProcessor
    : IDomainEventProcessor<AfterContentGeneratedEvent>
{
    /// <summary>
    /// Handles the logic to be executed after content has been generated.
    /// </summary>
    /// <param name="domainEvent">The event containing details about the generated content.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(AfterContentGeneratedEvent domainEvent)
    {
        // Logic for handling the event after content is generated
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
