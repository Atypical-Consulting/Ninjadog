namespace Ninjadog.Engine.Core.DomainEvents.ProcessTemplate;

/// <summary>
/// Represents an event that is triggered after a template has been parsed.
/// </summary>
/// <param name="Settings">The settings that will be used for processing.</param>
/// <param name="TemplateManifest">The template manifest that will be used for processing.</param>
/// <param name="ContextSnapshot">The context snapshot that will be used for processing.</param>
/// <param name="Template">The template that has been parsed.</param>
public record AfterTemplateParsedEvent(
    NinjadogSettings Settings,
    NinjadogTemplateManifest TemplateManifest,
    NinjadogEngineContextSnapshot ContextSnapshot,
    NinjadogTemplate Template)
    : NinjadogEngineEvent(Settings, TemplateManifest, ContextSnapshot);
