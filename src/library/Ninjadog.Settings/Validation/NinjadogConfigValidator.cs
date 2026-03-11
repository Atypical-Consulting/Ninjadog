// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Orchestrates validation of Ninjadog configuration files.
/// Performs schema validation first, then semantic validation if schema is valid.
/// </summary>
public static class NinjadogConfigValidator
{
    /// <summary>
    /// Validates a raw JSON string by first checking schema conformance,
    /// then parsing and running semantic checks.
    /// </summary>
    /// <param name="json">The raw JSON string to validate.</param>
    /// <returns>A <see cref="SchemaValidationResult"/> containing all diagnostics.</returns>
    public static SchemaValidationResult Validate(string json)
    {
        // Step 1: Schema validation
        var schemaResult = SchemaValidator.Validate(json);
        if (!schemaResult.IsValid)
        {
            return schemaResult;
        }

        // Step 2: Parse and run semantic validation
        NinjadogSettings settings;
        try
        {
            settings = NinjadogSettings.FromJsonString(json);
        }
        catch (Exception ex)
        {
            return new SchemaValidationResult(
                false,
                [
                    new ValidationDiagnostic(
                        "PARSE",
                        $"Failed to parse configuration: {ex.Message}",
                        ValidationSeverity.Error,
                        "$")
                ]);
        }

        return SemanticValidator.Validate(settings);
    }
}
