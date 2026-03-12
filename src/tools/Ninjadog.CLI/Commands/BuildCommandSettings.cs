using System.ComponentModel;

namespace Ninjadog.CLI.Commands;

internal sealed class BuildCommandSettings : CommandSettings
{
    [CommandOption("-v|--verbose")]
    [Description("Show detailed output including per-file generation info and dotnet CLI output.")]
    [DefaultValue(false)]
    public bool Verbose { get; init; }
}
