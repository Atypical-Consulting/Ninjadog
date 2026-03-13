using System.ComponentModel;

namespace Ninjadog.CLI.Commands;

internal sealed class EvolveCommandSettings : CommandSettings
{
    [CommandOption("-f|--file")]
    [Description("Path to the ninjadog.json file.")]
    public string? File { get; init; }

    [CommandOption("-n|--name")]
    [Description("A name for the migration file.")]
    public string? Name { get; init; }

    [CommandOption("--dry-run")]
    [Description("Preview changes without generating migration files.")]
    public bool DryRun { get; init; }
}
