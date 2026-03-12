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

    [CommandOption("--default")]
    [Description("Use default values without prompting")]
    public bool Default { get; init; }
}
