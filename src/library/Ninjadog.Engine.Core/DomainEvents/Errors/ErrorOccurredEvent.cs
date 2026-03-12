namespace Ninjadog.Engine.Core.DomainEvents.Errors;

/// <summary>
/// Represents an event that is triggered when an error occurs.
/// </summary>
/// <param name="Settings">The settings that will be used for processing.</param>
/// <param name="TemplateManifest">The template manifest that will be used for processing.</param>
/// <param name="ContextSnapshot">The context snapshot that will be used for processing.</param>
/// <param name="Exception">The exception that occurred.</param>
public record ErrorOccurredEvent(
    NinjadogSettings Settings,
    NinjadogTemplateManifest TemplateManifest,
    NinjadogEngineContextSnapshot ContextSnapshot,
    Exception Exception)
    : NinjadogEngineEvent(Settings, TemplateManifest, ContextSnapshot);
