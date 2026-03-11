// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Represents the result of validating a Ninjadog configuration.
/// </summary>
/// <param name="IsValid">Whether the configuration passed validation without errors.</param>
/// <param name="Diagnostics">The list of validation diagnostics found.</param>
public sealed record SchemaValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationDiagnostic> Diagnostics)
{
    /// <summary>
    /// Gets all diagnostics with <see cref="ValidationSeverity.Error"/> severity.
    /// </summary>
    public IReadOnlyList<ValidationDiagnostic> Errors
        => [.. Diagnostics.Where(d => d.Severity == ValidationSeverity.Error)];

    /// <summary>
    /// Gets all diagnostics with <see cref="ValidationSeverity.Warning"/> severity.
    /// </summary>
    public IReadOnlyList<ValidationDiagnostic> Warnings
        => [.. Diagnostics.Where(d => d.Severity == ValidationSeverity.Warning)];

    /// <summary>
    /// Creates a successful validation result with no diagnostics.
    /// </summary>
    /// <returns>A <see cref="SchemaValidationResult"/> with IsValid=true and empty diagnostics.</returns>
    public static SchemaValidationResult Success()
    {
        return new(true, []);
    }
}
