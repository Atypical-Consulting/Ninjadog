namespace Ninjadog.Engine.Core.DomainEvents.ProcessContent;

/// <summary>
/// Handles events that occur before content is generated.
/// </summary>
public class BeforeContentGeneratedProcessor
    : IDomainEventProcessor<BeforeContentGeneratedEvent>
{
    /// <summary>
    /// Handles the logic to be executed when content is about to be generated.
    /// </summary>
    /// <param name="domainEvent">The event containing details about the content to be generated.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(BeforeContentGeneratedEvent domainEvent)
    {
        // Logic for handling the event before content is generated
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
