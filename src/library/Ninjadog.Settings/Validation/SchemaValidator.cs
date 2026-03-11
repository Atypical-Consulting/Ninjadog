// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.Text.Json;
using Json.Schema;
using Ninjadog.Settings.Schema;

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Validates JSON content against the embedded ninjadog JSON schema.
/// </summary>
public static class SchemaValidator
{
    private static readonly Lazy<JsonSchema> _schema = new(
        () => JsonSchema.FromText(SchemaProvider.LoadSchemaJson()));

    /// <summary>
    /// Validates the given JSON content against the ninjadog schema.
    /// </summary>
    /// <param name="jsonContent">The raw JSON string to validate.</param>
    /// <returns>A list of validation diagnostics. Empty if the JSON is schema-valid.</returns>
    public static List<ValidationDiagnostic> Validate(string jsonContent)
    {
        var diagnostics = new List<ValidationDiagnostic>();

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(jsonContent);
        }
        catch (JsonException ex)
        {
            diagnostics.Add(new ValidationDiagnostic(
                "SCHEMA",
                $"Invalid JSON: {ex.Message}",
                ValidationSeverity.Error));
            return diagnostics;
        }

        using (document)
        {
            var options = new EvaluationOptions
            {
                OutputFormat = OutputFormat.List
            };

            var result = _schema.Value.Evaluate(document, options);

            if (!result.IsValid)
            {
                CollectErrors(result, diagnostics);
            }
        }

        return diagnostics;
    }

    private static void CollectErrors(EvaluationResults results, List<ValidationDiagnostic> diagnostics)
    {
        if (results.Details is null || results.Details.Count == 0)
        {
            if (!results.IsValid && results.Errors is not null)
            {
                foreach (var error in results.Errors)
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "SCHEMA",
                        error.Value,
                        ValidationSeverity.Error,
                        results.InstanceLocation.ToString()));
                }
            }

            return;
        }

        foreach (var detail in results.Details)
        {
            if (!detail.IsValid)
            {
                if (detail.Errors is not null)
                {
                    foreach (var error in detail.Errors)
                    {
                        diagnostics.Add(new ValidationDiagnostic(
                            "SCHEMA",
                            error.Value,
                            ValidationSeverity.Error,
                            detail.InstanceLocation.ToString()));
                    }
                }

                if (detail.Details is not null && detail.Details.Count > 0)
                {
                    CollectErrors(detail, diagnostics);
                }
            }
        }
    }
}
