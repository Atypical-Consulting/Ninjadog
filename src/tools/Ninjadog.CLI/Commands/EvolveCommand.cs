using System.Globalization;
using Ninjadog.Evolution;
using Ninjadog.Evolution.Migrations;

namespace Ninjadog.CLI.Commands;

internal sealed class EvolveCommand
    : Command<EvolveCommandSettings>
{
    public override int Execute(CommandContext context, EvolveCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var filePath = ResolveFilePath(settings.File);

            if (!File.Exists(filePath))
            {
                MarkupLine($"[red]Error:[/] File not found: [yellow]{filePath.EscapeMarkup()}[/]");
                return 2;
            }

            var projectRoot = Path.GetDirectoryName(Path.GetFullPath(filePath))!;
            var currentJson = File.ReadAllText(filePath);

            var currentSettings = AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(new Style(Color.Cyan))
                .Start("Loading ninjadog.json...", _ =>
                    NinjadogSettings.FromJsonString(currentJson, projectRoot));

            if (!SchemaState.Exists(projectRoot))
            {
                return HandleFirstRun(projectRoot, currentJson, settings.DryRun);
            }

            var previousJson = SchemaState.Load(projectRoot)!;
            var previousSettings = NinjadogSettings.FromJsonString(previousJson, projectRoot);

            var diff = SchemaDiffer.Compare(previousSettings, currentSettings);

            if (!diff.HasChanges)
            {
                MarkupLine("[green]No schema changes detected.[/]");
                return 0;
            }

            RenderDiff(diff);

            var operations = MigrationSqlGenerator.Generate(diff, currentSettings);

            if (operations.Count == 0)
            {
                MarkupLine("[dim]No SQL migration operations needed for these changes.[/]");
                SchemaState.Save(projectRoot, currentJson);
                return 0;
            }

            RenderOperations(operations);

            if (settings.DryRun)
            {
                WriteLine();
                MarkupLine("[yellow]Dry run — no files written.[/]");
                return 0;
            }

            var migrationPath = MigrationFileWriter.Write(projectRoot, operations, settings.Name);
            SchemaState.Save(projectRoot, currentJson);

            WriteLine();
            MarkupLine($"[bold green]Evolution complete.[/]");

            if (migrationPath is not null)
            {
                MarkupLine($"  Migration: [cyan]{Path.GetRelativePath(projectRoot, migrationPath).EscapeMarkup()}[/]");
            }

            MarkupLine("  State saved to [cyan].ninjadog/state.json[/]");

            return 0;
        }
        catch (Exception e)
        {
            WriteLine();
            WriteException(e);
            return 1;
        }
    }

    private static int HandleFirstRun(string projectRoot, string currentJson, bool dryRun)
    {
        MarkupLine("[yellow]No previous schema state found.[/]");
        MarkupLine("This appears to be the first time running [green]evolve[/].");
        WriteLine();

        if (dryRun)
        {
            MarkupLine("[yellow]Dry run — state would be saved as baseline.[/]");
            return 0;
        }

        SchemaState.Save(projectRoot, currentJson);
        MarkupLine("[green]Current schema saved as baseline.[/]");
        MarkupLine("Future runs of [green]ninjadog evolve[/] will compare against this snapshot.");
        return 0;
    }

    private static void RenderDiff(SchemaDiff diff)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Change[/]").Width(12))
            .AddColumn(new TableColumn("[bold]Target[/]"))
            .AddColumn(new TableColumn("[bold]Details[/]"));

        foreach (var entity in diff.AddedEntities)
        {
            var propCount = entity.PropertyChanges.Count;
            table.AddRow(
                "[green]+ Added[/]",
                $"Entity [bold]{entity.EntityKey.EscapeMarkup()}[/]",
                $"{propCount} properties");
        }

        foreach (var entity in diff.RemovedEntities)
        {
            table.AddRow(
                "[red]- Removed[/]",
                $"Entity [bold]{entity.EntityKey.EscapeMarkup()}[/]",
                string.Empty);
        }

        foreach (var entity in diff.ModifiedEntities)
        {
            var details = new List<string>();

            var added = entity.AddedProperties.Count();
            var removed = entity.RemovedProperties.Count();
            var modified = entity.ModifiedProperties.Count();

            if (added > 0)
            {
                details.Add($"+{added} props");
            }

            if (removed > 0)
            {
                details.Add($"-{removed} props");
            }

            if (modified > 0)
            {
                details.Add($"~{modified} props");
            }

            if (entity.RelationshipChanges.Count > 0)
            {
                details.Add($"{entity.RelationshipChanges.Count} rel changes");
            }

            table.AddRow(
                "[yellow]~ Modified[/]",
                $"Entity [bold]{entity.EntityKey.EscapeMarkup()}[/]",
                string.Join(", ", details).EscapeMarkup());
        }

        RenderConfigChanges(diff.ConfigChanges, table);
        RenderEnumChanges(diff.EnumChanges, table);

        WriteLine();
        Write(table);
        WriteLine();
    }

    private static void RenderConfigChanges(ConfigDiff config, Table table)
    {
        if (config.SoftDeleteChanged)
        {
            var state = config.SoftDeleteEnabled == true ? "enabled" : "disabled";
            table.AddRow("[yellow]~ Config[/]", "Soft Delete", state);
        }

        if (config.AuditingChanged)
        {
            var state = config.AuditingEnabled == true ? "enabled" : "disabled";
            table.AddRow("[yellow]~ Config[/]", "Auditing", state);
        }

        if (config.DatabaseProviderChanged)
        {
            table.AddRow(
                "[yellow]~ Config[/]",
                "Database Provider",
                $"{config.OldDatabaseProvider} -> {config.NewDatabaseProvider}");
        }

        if (config.AotChanged)
        {
            table.AddRow("[yellow]~ Config[/]", "AOT", "changed");
        }

        if (config.CorsChanged)
        {
            table.AddRow("[yellow]~ Config[/]", "CORS", "changed");
        }

        if (config.AuthChanged)
        {
            table.AddRow("[yellow]~ Config[/]", "Auth", "changed");
        }

        if (config.RateLimitChanged)
        {
            table.AddRow("[yellow]~ Config[/]", "Rate Limit", "changed");
        }

        if (config.VersioningChanged)
        {
            table.AddRow("[yellow]~ Config[/]", "Versioning", "changed");
        }
    }

    private static void RenderEnumChanges(IReadOnlyList<EnumDiff> enumChanges, Table table)
    {
        foreach (var e in enumChanges)
        {
            var (icon, details) = e.Kind switch
            {
                ChangeKind.Added => ("[green]+ Added[/]", FormatList(e.After)),
                ChangeKind.Removed => ("[red]- Removed[/]", string.Empty),
                ChangeKind.Modified => ("[yellow]~ Modified[/]", FormatEnumDelta(e)),
                _ => (string.Empty, string.Empty)
            };

            table.AddRow(icon, $"Enum [bold]{e.EnumName.EscapeMarkup()}[/]", details.EscapeMarkup());
        }
    }

    private static string FormatList(IReadOnlyList<string>? values)
    {
        return values is null ? string.Empty : string.Join(", ", values);
    }

    private static string FormatEnumDelta(EnumDiff e)
    {
        var parts = new List<string>();

        if (e.AddedValues.Count > 0)
        {
            parts.Add($"+{string.Join(", +", e.AddedValues)}");
        }

        if (e.RemovedValues.Count > 0)
        {
            parts.Add($"-{string.Join(", -", e.RemovedValues)}");
        }

        return string.Join("; ", parts);
    }

    private static void RenderOperations(IReadOnlyList<MigrationOperation> operations)
    {
        var opTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]#[/]").Width(4))
            .AddColumn(new TableColumn("[bold]Operation[/]"))
            .AddColumn(new TableColumn("[bold]Warning[/]").Width(8));

        for (var i = 0; i < operations.Count; i++)
        {
            var op = operations[i];
            var num = (i + 1).ToString(CultureInfo.InvariantCulture);
            var warning = op.IsWarning ? "[yellow]Yes[/]" : "[green]No[/]";
            opTable.AddRow(num, op.Description.EscapeMarkup(), warning);
        }

        Write(opTable);
    }

    private static string ResolveFilePath(string? file)
    {
        return !string.IsNullOrWhiteSpace(file)
            ? Path.IsPathRooted(file)
                ? file
                : Path.Combine(Directory.GetCurrentDirectory(), file)
            : Path.Combine(Directory.GetCurrentDirectory(), "ninjadog.json");
    }
}
