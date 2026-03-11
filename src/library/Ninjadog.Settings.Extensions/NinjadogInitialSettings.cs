// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using Ninjadog.Settings.Config;

namespace Ninjadog.Settings.Extensions;

/// <summary>
/// Represents the initial settings for the Ninjadog use case.
/// </summary>
public sealed record NinjadogInitialSettings : NinjadogSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NinjadogInitialSettings"/> class.
    /// </summary>
    /// <param name="name">The name of the Ninjadog app. Default is "NinjadogApp".</param>
    /// <param name="version">The version of the Ninjadog app. Default is "1.0.0".</param>
    /// <param name="description">The description of the Ninjadog app. Default is "Welcome to Ninjadog!".</param>
    /// <param name="rootNamespace">The root namespace of the Ninjadog app. Default is "NinjadogApp".</param>
    /// <param name="outputPath">The output path of the Ninjadog app. Defaults to ".".</param>
    /// <param name="saveGeneratedFiles">Whether to save generated files to disk. Default is true.</param>
    /// <param name="cors">The optional CORS configuration.</param>
    /// <param name="softDelete">Whether soft delete is enabled. Default is false.</param>
    /// <param name="auditing">Whether auditing is enabled. Default is false.</param>
    /// <param name="databaseProvider">The database provider to use. Default is "sqlite".</param>
    /// <param name="entities">Custom entities collection. If null, uses default initial entities.</param>
    public NinjadogInitialSettings(
        string? name = null,
        string? version = null,
        string? description = null,
        string? rootNamespace = null,
        string? outputPath = null,
        bool saveGeneratedFiles = true,
        NinjadogCorsConfiguration? cors = null,
        bool softDelete = false,
        bool auditing = false,
        string? databaseProvider = null,
        NinjadogEntities? entities = null)
        : base(
            new NinjadogInitialConfiguration(
                name,
                version,
                description,
                rootNamespace,
                outputPath,
                saveGeneratedFiles,
                cors,
                softDelete,
                auditing,
                databaseProvider),
            entities ?? new NinjadogInitialEntities())
    {
    }
}
