// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Config;

/// <summary>
/// A concrete implementation of <see cref="NinjadogConfiguration"/> used when loading configuration from a JSON file.
/// </summary>
public sealed record NinjadogLoadedConfiguration(
    string Name,
    string Version,
    string Description,
    string RootNamespace,
    string OutputPath,
    bool SaveGeneratedFiles = true,
    NinjadogCorsConfiguration? Cors = null)
    : NinjadogConfiguration(Name, Version, Description, RootNamespace, OutputPath, SaveGeneratedFiles, Cors);
