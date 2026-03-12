using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Entities.Properties;
using Ninjadog.Settings.Extensions;
using Ninjadog.Settings.Schema;

namespace Ninjadog.CLI.Commands;

internal sealed class InitCommand
    : Command<InitCommandSettings>
{
    private static readonly string[] _propertyTypes =
    [
        "Guid", "string", "int", "Int32", "long", "Int64",
        "bool", "Boolean", "decimal", "Decimal", "DateTime", "DateOnly"
    ];

    private static readonly string[] _databaseProviders =
    [
        "sqlite", "postgresql", "sqlserver"
    ];

    public override int Execute(CommandContext context, InitCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var initialSettings = ShouldPrompt(settings)
                ? CollectInteractiveSettings(settings)
                : BuildNonInteractiveSettings(settings);

            var json = initialSettings.ToJsonString();
            json = InjectSchema(json);

            File.WriteAllText("ninjadog.json", json);
            File.WriteAllText("ninjadog.schema.json", SchemaProvider.GetSchemaText());

            MarkupLine("[green]Ninjadog settings file created successfully.[/]");
            MarkupLine("[dim]  -> ninjadog.json[/]");
            MarkupLine("[dim]  -> ninjadog.schema.json[/]");

            return 0;
        }
        catch (Exception e)
        {
            WriteLine();
            WriteException(e);

            return 1;
        }
    }

    private static bool ShouldPrompt(InitCommandSettings settings)
    {
        return !settings.Default && !System.Console.IsInputRedirected;
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
