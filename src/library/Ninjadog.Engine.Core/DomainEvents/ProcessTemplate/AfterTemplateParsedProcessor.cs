namespace Ninjadog.Engine.Core.DomainEvents.ProcessTemplate;

/// <summary>
/// Handles events that occur after a template is parsed.
/// </summary>
public class AfterTemplateParsedProcessor
    : IDomainEventProcessor<AfterTemplateParsedEvent>
{
    /// <summary>
    /// Handles the logic to be executed after a template has been parsed.
    /// </summary>
    /// <param name="domainEvent">The event containing details about the parsed template.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(AfterTemplateParsedEvent domainEvent)
    {
        // Logic for handling the event after a template is generated
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
