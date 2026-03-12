using Ninjadog.Engine.Core.DomainEvents;
using static Ninjadog.CLI.Utilities.SpectreWriteHelpers;

namespace Ninjadog.CLI.Utilities;

internal sealed class NinjadogEngineEventDisplayService(IDomainEventDispatcher dispatcher, bool verbose = false)
    : NinjadogEngineEventSubscriber(dispatcher)
{
    private const string EnableMarkup = "[green]enabled[/]";
    private const string DisableMarkup = "[yellow]disabled[/]";

    private int _filesGeneratedForCurrentTemplate;
    private int _scaffoldRowIndex;

    private LiveDisplayContext? _liveContext;
    private Table? _liveTable;

    private bool HasLiveDisplay => _liveTable is not null;

    internal void SetLiveContext(LiveDisplayContext ctx, Table table)
    {
        _liveContext = ctx;
        _liveTable = table;
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
        else if (HasLiveDisplay)
        {
            _scaffoldRowIndex = _liveTable!.Rows.Count;
            _liveTable.AddRow("[yellow]...[/]", $"Scaffolding [green]{config.Name.EscapeMarkup()}[/]", string.Empty);
            _liveContext!.Refresh();
        }
    }

    protected override void ScaffoldingCompleted(ScaffoldingCompletedEvent domainEvent)
    {
        var manifest = domainEvent.TemplateManifest;

        if (verbose)
        {
            MarkupLine("[bold]Scaffolding complete. Generating files...[/]");
        }
        else if (HasLiveDisplay)
        {
            _liveTable!.Rows.Update(_scaffoldRowIndex, 0, new Markup("[green]OK[/]"));
            _liveTable.AddRow("[green]OK[/]", $"NuGet packages ({manifest.NuGetPackages.Count})", string.Empty);
            _liveContext!.Refresh();
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

        // Summary is printed after live display completes (in BuildCommand)
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
        else if (HasLiveDisplay)
        {
            var length = contentFile.Length;
            _liveTable!.AddRow(
                "[green]OK[/]",
                fileKey.EscapeMarkup(),
                $"[dim]{length:N0}[/]");
            _liveContext!.Refresh();
        }
    }

    protected override void OnErrorOccurred(ErrorOccurredEvent domainEvent)
    {
        var exception = domainEvent.Exception;

        if (HasLiveDisplay)
        {
            _liveTable!.AddRow("[bold red]ERR[/]", $"[red]{exception.Message.EscapeMarkup()}[/]", string.Empty);
            _liveContext!.Refresh();
        }
        else
        {
            WriteLine();
            MarkupLine("[bold red]Error occurred:[/]");
            WriteException(exception);
        }
    }
}
