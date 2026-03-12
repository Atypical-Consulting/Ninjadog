namespace Ninjadog.Engine.Core.DomainEvents.Run;

/// <summary>
/// Represents an event that is triggered after the engine has finished processing.
/// </summary>
/// <param name="Settings">The settings that will be used for processing.</param>
/// <param name="TemplateManifest">The template manifest that will be used for processing.</param>
/// <param name="ContextSnapshot">The context snapshot that will be used for processing.</param>
public record AfterEngineRunEvent(
    NinjadogSettings Settings,
    NinjadogTemplateManifest TemplateManifest,
    NinjadogEngineContextSnapshot ContextSnapshot)
    : NinjadogEngineEvent(Settings, TemplateManifest, ContextSnapshot);
