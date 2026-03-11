// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Represents the result of validating a ninjadog configuration file.
/// </summary>
/// <param name="IsValid">Whether the configuration passed all validation checks.</param>
/// <param name="Diagnostics">The list of diagnostics produced during validation.</param>
public sealed record SchemaValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationDiagnostic> Diagnostics);
