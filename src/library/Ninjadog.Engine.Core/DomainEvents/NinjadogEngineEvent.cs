namespace Ninjadog.Engine.Core.DomainEvents;

/// <summary>
/// Represents an event that is triggered by a class implementing <see cref="INinjadogEngine"/>.
/// </summary>
/// <param name="Settings">The settings that will be used for processing.</param>
/// <param name="TemplateManifest">The template manifest that will be used for processing.</param>
/// <param name="ContextSnapshot">The context snapshot that will be used for processing.</param>
public abstract record NinjadogEngineEvent(
    NinjadogSettings Settings,
    NinjadogTemplateManifest TemplateManifest,
    NinjadogEngineContextSnapshot ContextSnapshot)
    : DomainEvent;
