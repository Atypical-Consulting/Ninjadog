namespace Ninjadog.Engine.Core.DomainEvents.Run;

/// <summary>
/// Handles events that occur after scaffolding and NuGet package installation complete.
/// </summary>
public class ScaffoldingCompletedProcessor : IDomainEventProcessor<ScaffoldingCompletedEvent>
{
    /// <summary>
    /// Handles the logic to be executed when scaffolding has completed.
    /// </summary>
    /// <param name="domainEvent">The event containing details about the completed scaffolding.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(ScaffoldingCompletedEvent domainEvent)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
