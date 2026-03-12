namespace Ninjadog.Engine.Core.DomainEvents.ProcessContent;

/// <summary>
/// Represents an event that is triggered after content has been generated.
/// </summary>
/// <param name="Settings">The settings that will be used for processing.</param>
/// <param name="TemplateManifest">The template manifest that will be used for processing.</param>
/// <param name="ContextSnapshot">The context snapshot that will be used for processing.</param>
/// <param name="ContentFile">The content file that has been generated.</param>
public record AfterContentGeneratedEvent(
    NinjadogSettings Settings,
    NinjadogTemplateManifest TemplateManifest,
    NinjadogEngineContextSnapshot ContextSnapshot,
    NinjadogContentFile ContentFile)
    : NinjadogEngineEvent(Settings, TemplateManifest, ContextSnapshot);
