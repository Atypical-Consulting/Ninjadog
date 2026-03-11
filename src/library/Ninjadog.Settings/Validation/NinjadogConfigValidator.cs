// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Orchestrates schema and semantic validation of a ninjadog.json configuration file.
/// </summary>
public static class NinjadogConfigValidator
{
    /// <summary>
    /// Validates the given JSON content by running schema validation followed by semantic validation.
    /// </summary>
    /// <param name="jsonContent">The raw JSON string of the ninjadog.json file.</param>
    /// <returns>A <see cref="SchemaValidationResult"/> containing all diagnostics.</returns>
    public static SchemaValidationResult Validate(string jsonContent)
    {
        var diagnostics = new List<ValidationDiagnostic>();

        // Step 1: Schema validation
        var schemaDiagnostics = SchemaValidator.Validate(jsonContent);
        diagnostics.AddRange(schemaDiagnostics);

        // If schema validation failed with errors, skip semantic validation
        if (schemaDiagnostics.Any(d => d.Severity == ValidationSeverity.Error))
        {
            return new SchemaValidationResult(false, diagnostics);
        }

        // Step 2: Semantic validation (requires parseable JSON)
        try
        {
            var settings = NinjadogSettings.FromJsonString(jsonContent);
            var semanticDiagnostics = SemanticValidator.Validate(settings);
            diagnostics.AddRange(semanticDiagnostics);
        }
        catch (Exception ex)
        {
            diagnostics.Add(new ValidationDiagnostic(
                "PARSE",
                $"Failed to parse configuration: {ex.Message}",
                ValidationSeverity.Error));
        }

        var hasErrors = diagnostics.Any(d => d.Severity == ValidationSeverity.Error);
        return new SchemaValidationResult(!hasErrors, diagnostics);
    }
}
