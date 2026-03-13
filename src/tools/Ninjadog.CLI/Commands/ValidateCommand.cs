using Ninjadog.Settings.Validation;

namespace Ninjadog.CLI.Commands;

internal sealed class ValidateCommand
    : Command<ValidateCommandSettings>
{
    private const int ExitCodeValid = 0;
    private const int ExitCodeErrors = 1;
    private const int ExitCodeFileNotFound = 2;

    public override int Execute(CommandContext context, ValidateCommandSettings settings, CancellationToken cancellationToken)
    {
        var filePath = ResolveFilePath(settings.File);

        if (!File.Exists(filePath))
        {
            MarkupLine($"[red]Error:[/] File not found: [yellow]{filePath.EscapeMarkup()}[/]");
            return ExitCodeFileNotFound;
        }

        var result = AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(new Style(Color.Cyan))
            .Start($"Validating [cyan]{filePath.EscapeMarkup()}[/]...", _ =>
            {
                var jsonContent = File.ReadAllText(filePath);
                return NinjadogConfigValidator.Validate(jsonContent, Path.GetDirectoryName(Path.GetFullPath(filePath)));
            });

        WriteLine();

        return RenderResult(result, settings.Strict);
    }

    private static string ResolveFilePath(string? file)
    {
        return !string.IsNullOrWhiteSpace(file)
            ? Path.IsPathRooted(file)
                ? file
                : Path.Combine(Directory.GetCurrentDirectory(), file)
            : Path.Combine(Directory.GetCurrentDirectory(), "ninjadog.json");
    }

    private static int RenderResult(SchemaValidationResult result, bool strict)
    {
        var errors = result.Diagnostics
            .Where(d => d.Severity == ValidationSeverity.Error)
            .ToList();

        var warnings = result.Diagnostics
            .Where(d => d.Severity == ValidationSeverity.Warning)
            .ToList();

        var infos = result.Diagnostics
            .Where(d => d.Severity == ValidationSeverity.Info)
            .ToList();

        foreach (var diagnostic in errors)
        {
            RenderDiagnostic(diagnostic, "red", "x");
        }

        foreach (var diagnostic in warnings)
        {
            RenderDiagnostic(diagnostic, "yellow", "!");
        }

        foreach (var diagnostic in infos)
        {
            RenderDiagnostic(diagnostic, "blue", "i");
        }

        if (errors.Count > 0 || warnings.Count > 0 || infos.Count > 0)
        {
            WriteLine();
        }

        var hasErrors = errors.Count > 0;
        var hasStrictWarnings = strict && warnings.Count > 0;

        if (hasErrors || hasStrictWarnings)
        {
            var summary = new List<string>();
            if (errors.Count > 0)
            {
                summary.Add($"{errors.Count} error(s)");
            }

            if (warnings.Count > 0)
            {
                summary.Add($"{warnings.Count} warning(s)");
            }

            MarkupLine($"[red][[x]] Validation failed:[/] {string.Join(", ", summary)}");

            if (hasStrictWarnings && !hasErrors)
            {
                MarkupLine("[yellow]    (--strict mode treats warnings as errors)[/]");
            }

            return ExitCodeErrors;
        }

        if (warnings.Count > 0)
        {
            MarkupLine($"[green][[v]] Configuration is valid![/] [yellow]({warnings.Count} warning(s))[/]");
        }
        else
        {
            MarkupLine("[green][[v]] Configuration is valid![/]");
        }

        return ExitCodeValid;
    }

    private static void RenderDiagnostic(ValidationDiagnostic diagnostic, string color, string icon)
    {
        var path = diagnostic.Path is not null
            ? $" [dim]at {diagnostic.Path.EscapeMarkup()}[/]"
            : string.Empty;

        MarkupLine($"  [{color}][[{icon}]][/] [{color}]{diagnostic.Code}[/]: {diagnostic.Message.EscapeMarkup()}{path}");
    }
}
