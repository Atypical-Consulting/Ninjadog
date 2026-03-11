// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

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
