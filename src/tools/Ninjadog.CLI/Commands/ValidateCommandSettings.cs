using System.ComponentModel;

namespace Ninjadog.CLI.Commands;

internal sealed class ValidateCommandSettings : CommandSettings
{
    [CommandOption("-f|--file")]
    [Description("Path to the ninjadog.json file to validate.")]
    public string? File { get; init; }

    [CommandOption("--strict")]
    [Description("Treat warnings as errors.")]
    public bool Strict { get; init; }
}
