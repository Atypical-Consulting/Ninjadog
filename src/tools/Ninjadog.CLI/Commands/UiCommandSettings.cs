// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Ninjadog.CLI.Commands;

/// <summary>
/// Settings for the <c>ninjadog ui</c> command.
/// </summary>
public sealed class UiCommandSettings : CommandSettings
{
    /// <summary>
    /// Gets the port to run the web server on.
    /// </summary>
    [CommandOption("-p|--port")]
    [Description("Port to run the web server on")]
    [DefaultValue(5391)]
    public int Port { get; init; }

    /// <summary>
    /// Gets a value indicating whether the browser should not be opened automatically.
    /// </summary>
    [CommandOption("--no-browser")]
    [Description("Don't open browser automatically")]
    public bool NoBrowser { get; init; }
}
