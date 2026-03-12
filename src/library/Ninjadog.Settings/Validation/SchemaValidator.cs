// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.Text.Json;
using Json.Schema;
using Ninjadog.Settings.Schema;

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Validates raw JSON strings against the Ninjadog JSON Schema.
/// </summary>
public static class SchemaValidator
{
    /// <summary>
    /// Validates a JSON string against the Ninjadog schema.
    /// </summary>
    /// <param name="json">The raw JSON string to validate.</param>
    /// <returns>A <see cref="SchemaValidationResult"/> containing any schema violations.</returns>
    public static SchemaValidationResult Validate(string json)
    {
        try
        {
            JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            return new SchemaValidationResult(
                false,
                [
                    new ValidationDiagnostic(
                        "SCHEMA",
                        $"Invalid JSON: {ex.Message}",
                        ValidationSeverity.Error,
                        "$")
                ]);
        }

        var evaluationOptions = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        };

        var jsonElement = JsonDocument.Parse(json).RootElement;
        var result = SchemaProvider.Schema.Evaluate(jsonElement, evaluationOptions);

        if (result.IsValid)
        {
            return SchemaValidationResult.Success();
        }

        var diagnostics = new List<ValidationDiagnostic>();

        if (result.Details is not null)
        {
            foreach (var detail in result.Details)
            {
                if (detail.IsValid || detail.Errors is null || detail.Errors.Count == 0)
                {
                    continue;
                }

                var path = detail.InstanceLocation.ToString();

                foreach (var error in detail.Errors!)
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "SCHEMA",
                        error.Value,
                        ValidationSeverity.Error,
                        path));
                }
            }
        }

        // If we got no specific diagnostics but validation failed, add a generic one
        if (diagnostics.Count == 0)
        {
            diagnostics.Add(new ValidationDiagnostic(
                "SCHEMA",
                "JSON does not conform to the Ninjadog schema.",
                ValidationSeverity.Error,
                "$"));
        }

        return new SchemaValidationResult(false, diagnostics);
    }
}
