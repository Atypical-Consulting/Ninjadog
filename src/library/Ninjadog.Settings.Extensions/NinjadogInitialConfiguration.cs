// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using Ninjadog.Settings.Config;

namespace Ninjadog.Settings.Extensions;

/// <summary>
/// Provides the specific configuration for the "Ninjadog" application.
/// This sealed record inherits from NinjadogConfiguration and sets predefined values
/// tailored for the Ninjadog project, such as its name, version, description, and paths.
/// </summary>
public sealed record NinjadogInitialConfiguration : NinjadogConfiguration
{
    private const string DefaultName = "NinjadogApp";
    private const string DefaultVersion = "1.0.0";
    private const string DefaultDescription = "Welcome to Ninjadog!";
    private const string DefaultRootNamespace = "NinjadogApp";

    /// <summary>
    /// Initializes a new instance of the <see cref="NinjadogInitialConfiguration"/> class.
    /// </summary>
    /// <param name="name">The name of the Ninjadog app. Default is "NinjadogApp".</param>
    /// <param name="version">The version of the Ninjadog app. Default is "1.0.0".</param>
    /// <param name="description">The description of the Ninjadog app. Default is "Welcome to Ninjadog!".</param>
    /// <param name="rootNamespace">The root namespace of the Ninjadog app. Default is "NinjadogApp".</param>
    /// <param name="outputPath">The output path of the Ninjadog app. Defaults to ".".</param>
    /// <param name="saveGeneratedFiles">Whether to save generated files to disk. Default is true.</param>
    public NinjadogInitialConfiguration(
        string? name = null,
        string? version = null,
        string? description = null,
        string? rootNamespace = null,
        string? outputPath = null,
        bool saveGeneratedFiles = true)
        : base(
            Name: name ?? DefaultName,
            Version: version ?? DefaultVersion,
            Description: description ?? DefaultDescription,
            RootNamespace: rootNamespace ?? DefaultRootNamespace,
            OutputPath: outputPath ?? ".",
            SaveGeneratedFiles: saveGeneratedFiles)
    {
    }
}
