// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using Ninjadog.Engine.Core.DomainEvents;
using static Ninjadog.CLI.Utilities.SpectreWriteHelpers;

namespace Ninjadog.CLI.Utilities;

internal sealed class NinjadogEngineEventDisplayService(IDomainEventDispatcher dispatcher)
    : NinjadogEngineEventSubscriber(dispatcher)
{
    private const string EnableMarkup = "[green]enabled[/]";
    private const string DisableMarkup = "[yellow]disabled[/]";

    protected override void BeforeEngineRun(BeforeEngineRunEvent domainEvent)
    {
        var config = domainEvent.Settings.Config;
        var entities = domainEvent.Settings.Entities;
        var manifest = domainEvent.TemplateManifest;

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
        MarkupLine("[bold]Generating files...[/]");
    }

    protected override void AfterEngineRun(AfterEngineRunEvent domainEvent)
    {
        var snapshot = domainEvent.ContextSnapshot;
        var elapsed = snapshot.TotalTimeElapsed;
        var totalFilesGenerated = snapshot.TotalFilesGenerated;
        var totalCharactersGenerated = snapshot.TotalCharactersGenerated;
        var totalWordsGenerated = snapshot.TotalWordsGenerated;
        var totalMinutesSaved = snapshot.TotalMinutesSaved;

        WriteLine();

        MarkupLine("[bold]Ninjadog Engine run summary:[/]");
        MarkupLine($"- Run completed in [green]{elapsed:g}[/] seconds");
        MarkupLine($"- Total files generated: [green]{totalFilesGenerated:N0}[/] files");
        MarkupLine($"- Total characters generated in files: [green]{totalCharactersGenerated:N0}[/] characters");
        MarkupLine($"  - It represents ~[green]{totalWordsGenerated}[/] words or ~[green]{totalMinutesSaved}[/] minutes saved");
        WriteLine();

        MarkupLine("[bold]Ninjadog Engine shutting down.[/]");
        MarkupLine("[bold]Have a great day![/]");
        WriteLine();
    }

    protected override void BeforeTemplateGenerated(BeforeTemplateParsedEvent domainEvent)
    {
        var templateName = domainEvent.Template.Name;

        WriteLine();
        MarkupLine($"- Processing template [yellow]{templateName}[/]...");
    }

    protected override void AfterContentGenerated(AfterContentGeneratedEvent domainEvent)
    {
        var contentFile = domainEvent.ContentFile;
        var fileKey = contentFile.Key;
        var length = contentFile.Length;

        Write("  - File generated: ");
        WriteTextPath(fileKey);
        Markup($" with a length of [green]{length:N0}[/] characters.");
        WriteLine();
    }

    protected override void OnErrorOccurred(ErrorOccurredEvent domainEvent)
    {
        var exception = domainEvent.Exception;

        WriteLine();
        MarkupLine("[bold red]Error occurred:[/]");
        WriteException(exception);
    }
}
