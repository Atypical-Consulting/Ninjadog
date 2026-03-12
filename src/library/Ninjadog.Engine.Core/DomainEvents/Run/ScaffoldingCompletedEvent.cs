namespace Ninjadog.Engine.Core.DomainEvents.Run;

/// <summary>
/// Represents an event that is triggered after scaffolding and NuGet package installation complete.
/// </summary>
public record ScaffoldingCompletedEvent(
    NinjadogSettings Settings,
    NinjadogTemplateManifest TemplateManifest,
    NinjadogEngineContextSnapshot ContextSnapshot)
    : NinjadogEngineEvent(Settings, TemplateManifest, ContextSnapshot);
