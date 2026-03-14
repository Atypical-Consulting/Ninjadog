using Ninjadog.CLI.AI;
using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Entities.Properties;
using Ninjadog.Settings.Extensions;
using Ninjadog.Settings.Schema;
using Ninjadog.Settings.Validation;
using Ninjadog.Templates.CrudWebAPI.UseCases;

namespace Ninjadog.CLI.Commands;

internal sealed class InitCommand
    : AsyncCommand<InitCommandSettings>
{
    private static readonly string[] _propertyTypes =
    [
        "Guid", "string", "int", "long", "bool", "decimal", "DateTime", "DateOnly"
    ];

    private static readonly string[] _databaseProviders =
    [
        "sqlite", "postgresql", "sqlserver"
    ];

    private static readonly string[] _templates =
    [
        "CrudWebAPI"
    ];

    private static readonly string[] _useCases =
    [
        "TodoApp",
        "RestaurantBooking",
        "Custom"
    ];

    public override async Task<int> ExecuteAsync(CommandContext context, InitCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (settings.FromPrompt is not null)
            {
                return await ExecuteFromPromptAsync(settings.FromPrompt, cancellationToken);
            }

            ValidateCliArguments(settings);

            var resultSettings = ShouldPrompt(settings)
                ? CollectSettingsInteractively(settings)
                : ResolveUseCase(settings.UseCase, () => BuildNonInteractiveSettings(settings));

            var json = resultSettings.ToJsonString();
            json = InjectSchema(json);

            WriteConfigFiles(json);

            return 0;
        }
        catch (Exception e)
        {
            WriteLine();
            WriteException(e);

            return 1;
        }
    }

    private static async Task<int> ExecuteFromPromptAsync(string prompt, CancellationToken ct)
    {
        GenerationResult? result = null;

        await Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Generating configuration from prompt...", async _ =>
            {
                result = await ConfigGenerator.GenerateAsync(prompt, ct);
            });

        if (result is null || (!result.Success && result.Json is null))
        {
            MarkupLine($"[red]Error:[/] {result?.Error?.EscapeMarkup() ?? "Unknown error"}");
            return 1;
        }

        var json = InjectSchema(result.Json!);

        WriteConfigFiles(json);

        if (result.Validation is { IsValid: false })
        {
            WriteLine();
            MarkupLine("[yellow]Validation warnings:[/]");
            foreach (var diag in result.Validation.Diagnostics)
            {
                var color = diag.Severity == ValidationSeverity.Error ? "red" : "yellow";
                MarkupLine($"  [{color}]{diag.Path.EscapeMarkup()}:[/] {diag.Message.EscapeMarkup()}");
            }

            MarkupLine("[dim]The file was saved but may need manual adjustments.[/]");
        }

        return 0;
    }

    private static NinjadogSettings CollectSettingsInteractively(InitCommandSettings settings)
    {
        _ = SelectTemplate(settings);
        var useCase = SelectUseCase(settings);

        return ResolveUseCase(useCase, () => CollectInteractiveSettings(settings));
    }

    private static void ValidateCliArguments(InitCommandSettings settings)
    {
        if (settings.Template is not null && !_templates.Contains(settings.Template))
        {
            throw new InvalidOperationException(
                $"Unknown template '{settings.Template}'. Available: {string.Join(", ", _templates)}");
        }

        if (settings.UseCase is not null && !_useCases.Contains(settings.UseCase))
        {
            throw new InvalidOperationException(
                $"Unknown use case '{settings.UseCase}'. Available: {string.Join(", ", _useCases)}");
        }
    }

    private static NinjadogSettings ResolveUseCase(string? useCase, Func<NinjadogSettings> fallback)
    {
        return useCase switch
        {
            "TodoApp" => UseCaseSettings.TodoApp(),
            "RestaurantBooking" => UseCaseSettings.RestaurantBooking(),
            _ => fallback(),
        };
    }

    private static bool ShouldPrompt(InitCommandSettings settings)
    {
        return !settings.Default && !System.Console.IsInputRedirected;
    }

    private static string SelectTemplate(InitCommandSettings settings)
    {
        if (settings.Template is not null)
        {
            return settings.Template;
        }

        if (_templates.Length == 1)
        {
            MarkupLine($"[dim]Template:[/] [green]{_templates[0]}[/]");
            return _templates[0];
        }

        return Prompt(new SelectionPrompt<string>()
            .Title("[green]Template[/]:")
            .AddChoices(_templates));
    }

    private static string SelectUseCase(InitCommandSettings settings)
    {
        if (settings.UseCase is not null)
        {
            return settings.UseCase;
        }

        return Prompt(new SelectionPrompt<string>()
            .Title("[green]Use case[/]:")
            .AddChoices(_useCases));
    }

    private static NinjadogInitialSettings BuildNonInteractiveSettings(InitCommandSettings settings)
    {
        return new NinjadogInitialSettings(
            name: settings.Name,
            rootNamespace: settings.Namespace);
    }

    private static NinjadogInitialSettings CollectInteractiveSettings(InitCommandSettings settings)
    {
        MarkupLine("[bold blue]Ninjadog Project Setup[/]");
        MarkupLine("[dim]Answer the prompts below to configure your project.[/]");
        WriteLine();

        var defaultName = GetDirectoryName() ?? "MyNinjadogApp";

        var name = settings.Name
            ?? Prompt(new TextPrompt<string>("Project [green]name[/]:")
                .DefaultValue(defaultName));

        var rootNamespace = settings.Namespace
            ?? Prompt(new TextPrompt<string>("Root [green]namespace[/]:")
                .DefaultValue(name));

        var description = Prompt(new TextPrompt<string>("[green]Description[/]:")
            .DefaultValue("Welcome to Ninjadog!"));

        var databaseProvider = Prompt(new SelectionPrompt<string>()
            .Title("[green]Database provider[/]:")
            .AddChoices(_databaseProviders));

        var corsConfig = CollectCorsConfiguration();

        var softDelete = Confirm("[green]Enable soft delete?[/]", false);

        var auditing = Confirm("[green]Enable auditing?[/]", false);

        var entities = CollectEntities();

        return new NinjadogInitialSettings(
            name: name,
            description: description,
            rootNamespace: rootNamespace,
            databaseProvider: databaseProvider,
            cors: corsConfig,
            softDelete: softDelete,
            auditing: auditing,
            entities: entities);
    }

    private static NinjadogCorsConfiguration? CollectCorsConfiguration()
    {
        if (!Confirm("[green]Enable CORS?[/]", false))
        {
            return null;
        }

        var originsInput = Prompt(new TextPrompt<string>("  CORS [green]origins[/] (comma-separated):")
            .DefaultValue("http://localhost:3000"));

        var origins = originsInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new NinjadogCorsConfiguration(origins);
    }

    private static NinjadogLoadedEntities CollectEntities()
    {
        var entities = new NinjadogLoadedEntities();

        do
        {
            WriteLine();
            var entityName = Prompt(new TextPrompt<string>("Entity [green]name[/]:")
                .DefaultValue("Person"));

            var properties = CollectProperties();

            entities.Add(entityName, new NinjadogEntity(properties));
        }
        while (Confirm("Add [green]another entity[/]?", false));

        return entities;
    }

    private static NinjadogEntityProperties CollectProperties()
    {
        var properties = new NinjadogEntityProperties();

        do
        {
            var propName = Prompt(new TextPrompt<string>("  Property [green]name[/]:"));

            var propType = Prompt(new SelectionPrompt<string>()
                .Title($"  Property [green]type[/] for {propName}:")
                .AddChoices(_propertyTypes));

            var isKey = Confirm($"  Is [green]{propName}[/] a key?", false);

            var required = Confirm($"  Is [green]{propName}[/] required?", false);

            properties.Add(propName, new NinjadogEntityProperty(propType, isKey, required));
        }
        while (Confirm("  Add [green]another property[/]?", true));

        return properties;
    }

    private static void WriteConfigFiles(string json)
    {
        File.WriteAllText("ninjadog.json", json);
        File.WriteAllText("ninjadog.schema.json", SchemaProvider.GetSchemaText());

        MarkupLine("[green]Ninjadog settings file created successfully.[/]");
        MarkupLine("[dim]  -> ninjadog.json[/]");
        MarkupLine("[dim]  -> ninjadog.schema.json[/]");
    }

    private static string? GetDirectoryName()
    {
        try
        {
            return Path.GetFileName(Directory.GetCurrentDirectory());
        }
        catch
        {
            return null;
        }
    }

    private static string InjectSchema(string json)
    {
        const string schemaLine = "  \"$schema\": \"./ninjadog.schema.json\",";
        var openBrace = json.IndexOf('{');

        return openBrace < 0
            ? json
            : json.Insert(openBrace + 1, "\n" + schemaLine);
    }
}
