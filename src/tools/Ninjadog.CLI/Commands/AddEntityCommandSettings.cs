using System.ComponentModel;

namespace Ninjadog.CLI.Commands;

internal sealed class AddEntityCommandSettings : CommandSettings
{
    [Description("The name of the entity to add (PascalCase).")]
    [CommandArgument(0, "<entityName>")]
    public string EntityName { get; init; } = default!;
}
