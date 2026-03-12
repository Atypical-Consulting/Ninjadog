namespace Ninjadog.Engine.Core.DomainEvents;

/// <summary>
/// Provides an abstract base class for subscribing to various events within the Ninjadog Engine.
/// This class allows for custom handling of different lifecycle events of the engine.
/// </summary>
/// <param name="dispatcher">The dispatcher used to register event handlers.</param>
public abstract class NinjadogEngineEventSubscriber(IDomainEventDispatcher dispatcher)
{
    /// <summary>
    /// Registers all the event handlers for engine lifecycle events.
    /// This method is called to connect the various event handling methods with their respective events.
    /// </summary>
    public virtual void RegisterAllHandlers()
    {
        RegisterOnBeforeEngineRun();
        RegisterOnScaffoldingCompleted();
        RegisterOnAfterEngineRun();
        RegisterOnBeforeTemplateGenerated();
        RegisterOnAfterTemplateGenerated();
        RegisterOnBeforeContentGenerated();
        RegisterOnAfterContentGenerated();
        RegisterOnErrorOccurred();
    }

    /// <summary>
    /// Handles the event triggered before the engine run starts.
    /// This method can be overridden to perform custom actions at the start of the engine run.
    /// </summary>
    /// <param name="domainEvent">The event data for the 'BeforeEngineRun' event.</param>
    protected virtual void BeforeEngineRun(BeforeEngineRunEvent domainEvent)
    {
        // Do nothing
    }

    /// <summary>
    /// Handles the event triggered after scaffolding and NuGet package installation complete.
    /// Override to perform custom actions when the project structure is ready.
    /// </summary>
    /// <param name="domainEvent">The event data for the 'ScaffoldingCompleted' event.</param>
    protected virtual void ScaffoldingCompleted(ScaffoldingCompletedEvent domainEvent)
    {
        // Do nothing
    }

    /// <summary>
    /// Handles the event triggered after the engine run completes.
    /// This method can be overridden to perform custom actions at the end of the engine run.
    /// </summary>
    /// <param name="domainEvent">The event data for the 'AfterEngineRun' event.</param>
    protected virtual void AfterEngineRun(AfterEngineRunEvent domainEvent)
    {
        // Do nothing
    }

    /// <summary>
    /// Handles the event triggered before a template is generated.
    /// Override this method to perform actions before each template generation starts.
    /// </summary>
    /// <param name="domainEvent">The event data for the 'BeforeTemplateGenerated' event.</param>
    protected virtual void BeforeTemplateGenerated(BeforeTemplateParsedEvent domainEvent)
    {
        // Do nothing
    }

    /// <summary>
    /// Handles the event triggered after a template is generated.
    /// Override this method to perform actions after each template generation completes.
    /// </summary>
    /// <param name="domainEvent">The event data for the 'AfterTemplateGenerated' event.</param>
    protected virtual void AfterTemplateGenerated(AfterTemplateParsedEvent domainEvent)
    {
        // Do nothing
    }

    /// <summary>
    /// Handles the event triggered before content generation based on a template.
    /// Override to perform custom actions before each piece of content is generated.
    /// </summary>
    /// <param name="domainEvent">The event data for the 'BeforeContentGenerated' event.</param>
    protected virtual void BeforeContentGenerated(BeforeContentGeneratedEvent domainEvent)
    {
        // Do nothing
    }

    /// <summary>
    /// Handles the event triggered after content is generated from a template.
    /// Override to perform custom actions after each piece of content is generated.
    /// </summary>
    /// <param name="domainEvent">The event data for the 'AfterContentGenerated' event.</param>
    protected virtual void AfterContentGenerated(AfterContentGeneratedEvent domainEvent)
    {
        // Do nothing
    }

    /// <summary>
    /// Handles the event triggered when an error occurs during the engine run.
    /// Override this method to perform custom error handling.
    /// </summary>
    /// <param name="domainEvent">The event data for the 'ErrorOccurred' event.</param>
    protected virtual void OnErrorOccurred(ErrorOccurredEvent domainEvent)
    {
        // Do nothing
    }

    private void RegisterOnBeforeEngineRun()
    {
        dispatcher.RegisterHandler((BeforeEngineRunEvent domainEvent) =>
        {
            BeforeEngineRun(domainEvent);
            return Task.CompletedTask;
        });
    }

    private void RegisterOnScaffoldingCompleted()
    {
        dispatcher.RegisterHandler((ScaffoldingCompletedEvent domainEvent) =>
        {
            ScaffoldingCompleted(domainEvent);
            return Task.CompletedTask;
        });
    }

    private void RegisterOnAfterEngineRun()
    {
        dispatcher.RegisterHandler((AfterEngineRunEvent domainEvent) =>
        {
            AfterEngineRun(domainEvent);
            return Task.CompletedTask;
        });
    }

    private void RegisterOnBeforeTemplateGenerated()
    {
        dispatcher.RegisterHandler((BeforeTemplateParsedEvent domainEvent) =>
        {
            BeforeTemplateGenerated(domainEvent);
            return Task.CompletedTask;
        });
    }

    private void RegisterOnAfterTemplateGenerated()
    {
        dispatcher.RegisterHandler((AfterTemplateParsedEvent domainEvent) =>
        {
            AfterTemplateGenerated(domainEvent);
            return Task.CompletedTask;
        });
    }

    private void RegisterOnBeforeContentGenerated()
    {
        dispatcher.RegisterHandler((BeforeContentGeneratedEvent domainEvent) =>
        {
            BeforeContentGenerated(domainEvent);
            return Task.CompletedTask;
        });
    }

    private void RegisterOnAfterContentGenerated()
    {
        dispatcher.RegisterHandler((AfterContentGeneratedEvent domainEvent) =>
        {
            AfterContentGenerated(domainEvent);
            return Task.CompletedTask;
        });
    }

    private void RegisterOnErrorOccurred()
    {
        dispatcher.RegisterHandler((ErrorOccurredEvent domainEvent) =>
        {
            OnErrorOccurred(domainEvent);
            return Task.CompletedTask;
        });
    }
}
