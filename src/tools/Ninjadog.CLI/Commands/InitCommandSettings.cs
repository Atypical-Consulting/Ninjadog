using System.ComponentModel;

namespace Ninjadog.CLI.Commands;

internal sealed class InitCommandSettings : CommandSettings
{
    [CommandOption("-n|--name")]
    [Description("Project name (skips prompt)")]
    public string? Name { get; init; }

    [CommandOption("--namespace")]
    [Description("Root namespace (skips prompt)")]
    public string? Namespace { get; init; }

    [CommandOption("-t|--template")]
    [Description("Template to use (e.g. CrudWebAPI)")]
    public string? Template { get; init; }

    [CommandOption("-u|--use-case")]
    [Description("Use case to scaffold (TodoApp, RestaurantBooking, or Custom)")]
    public string? UseCase { get; init; }

    [CommandOption("--default")]
    [Description("Use default values without prompting")]
    public bool Default { get; init; }

    [CommandOption("--from-prompt")]
    [Description("Generate config from a natural language description using AI (requires ANTHROPIC_API_KEY)")]
    public string? FromPrompt { get; init; }
}
