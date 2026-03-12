namespace Ninjadog.Engine.Core.DomainEvents.Run;

/// <summary>
/// Represents an event that is triggered before the engine starts processing.
/// </summary>
public record BeforeEngineRunEvent(
    NinjadogSettings Settings,
    NinjadogTemplateManifest TemplateManifest,
    NinjadogEngineContextSnapshot ContextSnapshot)
    : NinjadogEngineEvent(Settings, TemplateManifest, ContextSnapshot);
