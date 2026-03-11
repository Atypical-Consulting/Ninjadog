// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Ninjadog.CLI.Commands;

internal sealed class AddEntityCommandSettings : CommandSettings
{
    [Description("The name of the entity to add (PascalCase).")]
    [CommandArgument(0, "<entityName>")]
    public string EntityName { get; init; } = default!;
}
