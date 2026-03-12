using Ninjadog.Engine.Core.ValueObjects;

namespace Ninjadog.Engine;

/// <summary>
/// Represents the Ninjadog Engine, which handles the execution of template generation processes.
/// This class encapsulates the core functionality of running the templating engine to produce output based on specified templates.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NinjadogEngine"/> class.
/// </remarks>
/// <param name="templateManifest">The template manifest to be used by the engine.</param>
/// <param name="ninjadogSettings">The ninjadog app settings to configure the engine.</param>
/// <param name="outputProcessors">The output processors to be used by the engine.</param>
/// <param name="ninjadogAppService">The ninjadog app service to be used by the engine.</param>
/// <param name="domainEventDispatcher">The domain event dispatcher to be used by the engine.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
public sealed class NinjadogEngine(
    NinjadogTemplateManifest templateManifest,
    NinjadogSettings ninjadogSettings,
    NinjadogOutputProcessors outputProcessors,
    INinjadogAppService ninjadogAppService,
    IDomainEventDispatcher domainEventDispatcher)
    : INinjadogEngine
{
    /// <inheritdoc />
    public NinjadogEngineContext Context { get; } = new();

    /// <inheritdoc />
    public ICollection<IDomainEvent> Events { get; } = [];

    /// <inheritdoc />
    public void Run()
    {
        try
        {
            DispatchBeforeEngineRun();

            // process each template provided by the manifest
            foreach (var template in templateManifest.Templates)
            {
                ProcessTemplate(template);
            }
        }
        catch (Exception ex)
        {
            DispatchError(ex);
        }
        finally
        {
            DispatchAfterEngineRun();
        }
    }

    private void ProcessTemplate(NinjadogTemplate template)
    {
        try
        {
            DispatchBeforeEngineTemplateParsed(template);

            // First, add a single file based on the template and the settings...
            var singleFileContent = template.GenerateOne(ninjadogSettings);
            ProcessContent(singleFileContent);

            // ...then, add multiple files based on the template and the settings
            foreach (var content in template.GenerateMany(ninjadogSettings))
            {
                ProcessContent(content);
            }
        }
        catch (Exception ex)
        {
            DispatchError(ex);
        }
        finally
        {
            DispatchAfterTemplateParsed(template);
        }
    }

    private void ProcessContent(NinjadogContentFile contentFile)
    {
        if (contentFile.IsEmpty)
        {
            return;
        }

        DispatchBeforeContentGenerated(contentFile);

        foreach (var processor in outputProcessors)
        {
            processor.ProcessOutput(templateManifest, ninjadogSettings, contentFile);
        }

        DispatchAfterContentGenerated(contentFile);
    }

    // Lifecycle methods
    private void DispatchBeforeEngineRun()
    {
        // reset the context and start collecting metrics
        Context.Reset();
        Context.StartCollectMetrics();

        // notify subscribers that the engine is about to start scaffolding
        var beforeSnapshot = Context.GetSnapshot();
        BeforeEngineRunEvent beforeEvent = new(ninjadogSettings, templateManifest, beforeSnapshot);
        domainEventDispatcher.Dispatch(beforeEvent);

        // create the app folder and the initial files (sln, .gitignore, ninjadog.json etc.)
        ninjadogAppService.DotnetVersionAsync().Wait();
        ninjadogAppService.CreateAppAsync().Wait();

        // notify subscribers that scaffolding is complete
        var afterSnapshot = Context.GetSnapshot();
        ScaffoldingCompletedEvent scaffoldingEvent = new(ninjadogSettings, templateManifest, afterSnapshot);
        domainEventDispatcher.Dispatch(scaffoldingEvent);
    }

    private void DispatchAfterEngineRun()
    {
        Context.StopCollectMetrics();
        var snapshot = Context.GetSnapshot();
        AfterEngineRunEvent domainEvent = new(ninjadogSettings, templateManifest, snapshot);
        domainEventDispatcher.Dispatch(domainEvent);
    }

    private void DispatchBeforeEngineTemplateParsed(NinjadogTemplate template)
    {
        var snapshot = Context.GetSnapshot();
        BeforeTemplateParsedEvent domainEvent = new(ninjadogSettings, templateManifest, snapshot, template);
        domainEventDispatcher.Dispatch(domainEvent);
    }

    private void DispatchAfterTemplateParsed(NinjadogTemplate template)
    {
        var snapshot = Context.GetSnapshot();
        AfterTemplateParsedEvent domainEvent = new(ninjadogSettings, templateManifest, snapshot, template);
        domainEventDispatcher.Dispatch(domainEvent);
    }

    private void DispatchBeforeContentGenerated(NinjadogContentFile contentFile)
    {
        var snapshot = Context.GetSnapshot();
        BeforeContentGeneratedEvent domainEvent = new(ninjadogSettings, templateManifest, snapshot, contentFile);
        domainEventDispatcher.Dispatch(domainEvent);
    }

    private void DispatchAfterContentGenerated(NinjadogContentFile contentFile)
    {
        Context.FileGenerated(contentFile);
        var snapshot = Context.GetSnapshot();
        AfterContentGeneratedEvent domainEvent = new(ninjadogSettings, templateManifest, snapshot, contentFile);
        domainEventDispatcher.Dispatch(domainEvent);
    }

    private void DispatchError(Exception exception)
    {
        Context.ErrorOccurred(exception);
        var snapshot = Context.GetSnapshot();
        ErrorOccurredEvent domainEvent = new(ninjadogSettings, templateManifest, snapshot, exception);
        domainEventDispatcher.Dispatch(domainEvent);
    }
}
