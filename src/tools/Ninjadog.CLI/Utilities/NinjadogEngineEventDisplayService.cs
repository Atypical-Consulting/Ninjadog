using Ninjadog.Engine.Core.DomainEvents;
using static Ninjadog.CLI.Utilities.SpectreWriteHelpers;

namespace Ninjadog.CLI.Utilities;

internal sealed class NinjadogEngineEventDisplayService(IDomainEventDispatcher dispatcher, bool verbose = false)
    : NinjadogEngineEventSubscriber(dispatcher)
{
    private const string EnableMarkup = "[green]enabled[/]";
    private const string DisableMarkup = "[yellow]disabled[/]";

    private int _filesGeneratedForCurrentTemplate;

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
        else
        {
            Markup($"Scaffolding [green]{config.Name}[/] project...");
        }
    }

    protected override void ScaffoldingCompleted(ScaffoldingCompletedEvent domainEvent)
    {
        var entities = domainEvent.Settings.Entities;
        var manifest = domainEvent.TemplateManifest;

        if (verbose)
        {
            MarkupLine("[bold]Scaffolding complete. Generating files...[/]");
        }
        else
        {
            var entityLabel = entities.Count == 1 ? "entity" : "entities";
            MarkupLine($"            [green]done[/]");
            MarkupLine($"Adding NuGet packages ({manifest.NuGetPackages.Count})...            [green]done[/]");
            MarkupLine($"Generating files for {entities.Count} {entityLabel}...");
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
        else
        {
            MarkupLine($"Generating files...                     [green]done[/]");
            WriteLine();
            MarkupLine($"[bold]Build completed in {elapsed.TotalSeconds:F1}s[/] — [green]{totalFilesGenerated} files[/] generated");
            WriteLine();
        }
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
        if (verbose && _filesGeneratedForCurrentTemplate == 0)
        {
            MarkupLine("  [yellow](skipped)[/]");
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
        else
        {
            var fileNumber = domainEvent.ContextSnapshot.TotalFilesGenerated;
            Markup($"  [grey][[{fileNumber}]][/] ");
            WriteTextPath(fileKey);
            WriteLine();
        }
    }

    protected override void OnErrorOccurred(ErrorOccurredEvent domainEvent)
    {
        var exception = domainEvent.Exception;

        WriteLine();
        MarkupLine("[bold red]Error occurred:[/]");
        WriteException(exception);
    }
}
