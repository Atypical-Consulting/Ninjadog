using Ninjadog.Engine.Core.DomainEvents;
using static Ninjadog.CLI.Utilities.SpectreWriteHelpers;

namespace Ninjadog.CLI.Utilities;

internal sealed class NinjadogEngineEventDisplayService(IDomainEventDispatcher dispatcher, bool verbose = false)
    : NinjadogEngineEventSubscriber(dispatcher)
{
    private const string EnableMarkup = "[green]enabled[/]";
    private const string DisableMarkup = "[yellow]disabled[/]";

    private int _filesGeneratedForCurrentTemplate;

    private ProgressContext? _progressContext;
    private ProgressTask? _scaffoldTask;
    private ProgressTask? _nugetTask;
    private ProgressTask? _generateTask;

    internal void SetProgressContext(ProgressContext ctx)
    {
        _progressContext = ctx;
    }

    protected override void BeforeEngineRun(BeforeEngineRunEvent domainEvent)
    {
        var config = domainEvent.Settings.Config;
        var entities = domainEvent.Settings.Entities;
        var manifest = domainEvent.TemplateManifest;

        if (verbose)
        {
            MarkupLine("[bold]Using the following settings:[/]");
            WriteSettingsTable(table => table
                .AddRow("Engine", $"[green]Ninjadog.Engine[/] v2.0.0-alpha")
                .AddRow("Template", $"[green]{manifest.Name}[/] v{manifest.Version}")
                .AddRow("App name", $"[green]{config.Name}[/] v{config.Version} with [green]{entities.Count}[/] entities")
                .AddRow("App entities", $"[green]{string.Join(", ", entities.Keys)}[/]")
                .AddRow("Authentication", DisableMarkup)
                .AddRow("Persistence", "[green]SQLite[/]"));
            WriteLine();

            MarkupLine("[bold]Output processors:[/]");
            WriteSettingsTable(table => table
                .AddRow("InMemory", EnableMarkup)
                .AddRow("Disk", EnableMarkup)
                .AddRow("Zip", DisableMarkup));
            WriteLine();

            MarkupLine("[bold]Integrations:[/]");
            WriteSettingsTable(table => table
                .AddRow("Setup .NET Aspire", DisableMarkup)
                .AddRow("Create Dockerfile", EnableMarkup)
                .AddRow("Create Git repository", DisableMarkup)
                .AddRow("Create GitHub Actions", DisableMarkup)
                .AddRow("Push on GitHub", DisableMarkup));
            WriteLine();

            WriteLine();
            MarkupLine("[bold]Scaffolding project...[/]");
        }
        else if (_progressContext is not null)
        {
            _scaffoldTask = _progressContext.AddTask($"Scaffolding [green]{config.Name.EscapeMarkup()}[/]", maxValue: 1);
            _nugetTask = _progressContext.AddTask($"Adding NuGet packages ({manifest.NuGetPackages.Count})", autoStart: false, maxValue: 1);
            _generateTask = _progressContext.AddTask("Generating files", autoStart: false, maxValue: manifest.Templates.Count);
        }
    }

    protected override void ScaffoldingCompleted(ScaffoldingCompletedEvent domainEvent)
    {
        if (verbose)
        {
            MarkupLine("[bold]Scaffolding complete. Generating files...[/]");
        }
        else if (_progressContext is not null)
        {
            _scaffoldTask?.Increment(1);

            _nugetTask?.StartTask();
            _nugetTask?.Increment(1);

            _generateTask?.StartTask();
        }
    }

    protected override void AfterEngineRun(AfterEngineRunEvent domainEvent)
    {
        var snapshot = domainEvent.ContextSnapshot;
        var elapsed = snapshot.TotalTimeElapsed;
        var totalFilesGenerated = snapshot.TotalFilesGenerated;

        if (verbose)
        {
            WriteLine();
            MarkupLine("[bold]Ninjadog Engine run summary:[/]");
            MarkupLine($"- Run completed in [green]{elapsed.TotalSeconds:F1}s[/]");
            MarkupLine($"- Total files generated: [green]{totalFilesGenerated:N0}[/] files");
            WriteLine();
        }

        // Summary is printed after progress completes (in BuildCommand)
    }

    protected override void BeforeTemplateGenerated(BeforeTemplateParsedEvent domainEvent)
    {
        _filesGeneratedForCurrentTemplate = 0;

        if (verbose)
        {
            var templateName = domainEvent.Template.Name;
            WriteLine();
            MarkupLine($"- Processing template [yellow]{templateName}[/]...");
        }
        else if (_generateTask is not null)
        {
            var templateName = domainEvent.Template.Name;
            _generateTask.Description = $"Generating [yellow]{templateName.EscapeMarkup()}[/]";
            _progressContext?.Refresh();
        }
    }

    protected override void AfterTemplateGenerated(AfterTemplateParsedEvent domainEvent)
    {
        if (verbose)
        {
            if (_filesGeneratedForCurrentTemplate == 0)
            {
                MarkupLine("  [yellow](skipped)[/]");
            }
        }
        else
        {
            _generateTask?.Increment(1);
        }
    }

    protected override void AfterContentGenerated(AfterContentGeneratedEvent domainEvent)
    {
        _filesGeneratedForCurrentTemplate++;

        var contentFile = domainEvent.ContentFile;
        var fileKey = contentFile.Key;

        if (verbose)
        {
            var length = contentFile.Length;

            Write("  - File generated: ");
            WriteTextPath(fileKey);
            Markup($" with a length of [green]{length:N0}[/] characters.");
            WriteLine();
        }

        // In non-verbose mode with progress, individual files are tracked via the progress bar
    }

    protected override void OnErrorOccurred(ErrorOccurredEvent domainEvent)
    {
        var exception = domainEvent.Exception;

        WriteLine();
        MarkupLine("[bold red]Error occurred:[/]");
        WriteException(exception);
    }
}
