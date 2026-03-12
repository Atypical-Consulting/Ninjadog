namespace Ninjadog.Engine.Core.DomainEvents.Errors;

/// <summary>
/// Handles events that occur when an error occurs.
/// </summary>
public class ErrorOccurredProcessor
    : IDomainEventProcessor<ErrorOccurredEvent>
{
    /// <summary>
    /// Handles the logic to be executed when an error occurs.
    /// </summary>
    /// <param name="domainEvent">The event containing details about the error.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(ErrorOccurredEvent domainEvent)
    {
        // Logic for handling the event when an error occurs
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
