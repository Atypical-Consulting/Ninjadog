// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Validates parsed <see cref="NinjadogSettings"/> for semantic correctness.
/// Implements checks NINJ001 through NINJ010.
/// </summary>
public static partial class SemanticValidator
{
    private static readonly HashSet<string> _builtInTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string",
        "int", "Int32",
        "long", "Int64",
        "float", "Single",
        "double",
        "decimal",
        "bool", "Boolean",
        "Guid",
        "DateTime", "DateTimeOffset",
        "DateOnly", "TimeOnly",
        "byte[]"
    };

    /// <summary>
    /// Validates the given settings for semantic correctness.
    /// </summary>
    /// <param name="settings">The parsed Ninjadog settings to validate.</param>
    /// <returns>A <see cref="SchemaValidationResult"/> containing any semantic issues.</returns>
    public static SchemaValidationResult Validate(NinjadogSettings settings)
    {
        var diagnostics = new List<ValidationDiagnostic>();

        foreach (var (entityName, entity) in settings.Entities)
        {
            CheckNinj001KeyProperty(diagnostics, entityName, entity);
            CheckNinj002RelatedEntity(diagnostics, entityName, entity, settings);
            CheckNinj003SeedDataFieldNames(diagnostics, entityName, entity);
            CheckNinj004SeedDataTypes(diagnostics, entityName, entity);
            CheckNinj005NonBuiltInType(diagnostics, entityName, entity, settings);
            CheckNinj006MinMaxLength(diagnostics, entityName, entity);
            CheckNinj007MinMax(diagnostics, entityName, entity);
            CheckNinj008PascalCase(diagnostics, entityName);
            CheckNinj010SeedDataKeyUniqueness(diagnostics, entityName, entity);
        }

        CheckNinj009EnumUniqueness(diagnostics, settings);

        var hasErrors = diagnostics.Any(d => d.Severity == ValidationSeverity.Error);
        return new SchemaValidationResult(!hasErrors, diagnostics);
    }

    private static void CheckNinj001KeyProperty(
        List<ValidationDiagnostic> diagnostics,
        string entityName,
        Entities.NinjadogEntity entity)
    {
        var keyCount = entity.Properties.Count(p => p.Value.IsKey);

        if (keyCount != 1)
        {
            diagnostics.Add(new ValidationDiagnostic(
                "NINJ001",
                $"Entity '{entityName}' must have exactly one isKey property, but found {keyCount}.",
                ValidationSeverity.Error,
                $"$.entities.{entityName}.properties"));
        }
    }

    private static void CheckNinj002RelatedEntity(
        List<ValidationDiagnostic> diagnostics,
        string entityName,
        Entities.NinjadogEntity entity,
        NinjadogSettings settings)
    {
        if (entity.Relationships is null)
        {
            return;
        }

        foreach (var (relName, rel) in entity.Relationships)
        {
            if (!settings.Entities.ContainsKey(rel.RelatedEntity))
            {
                diagnostics.Add(new ValidationDiagnostic(
                    "NINJ002",
                    $"Relationship '{relName}' in entity '{entityName}' references non-existent entity '{rel.RelatedEntity}'.",
                    ValidationSeverity.Error,
                    $"$.entities.{entityName}.relationships.{relName}.relatedEntity"));
            }
        }
    }

    private static void CheckNinj003SeedDataFieldNames(
        List<ValidationDiagnostic> diagnostics,
        string entityName,
        Entities.NinjadogEntity entity)
    {
        if (entity.SeedData is null)
        {
            return;
        }

        var propertyNames = new HashSet<string>(entity.Properties.Keys);

        for (var i = 0; i < entity.SeedData.Count; i++)
        {
            var row = entity.SeedData[i];
            foreach (var fieldName in row.Keys)
            {
                if (!propertyNames.Contains(fieldName))
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "NINJ003",
                        $"Seed data field '{fieldName}' in entity '{entityName}' at index {i} does not match any property.",
                        ValidationSeverity.Error,
                        $"$.entities.{entityName}.seedData[{i}].{fieldName}"));
                }
            }
        }
    }

    private static void CheckNinj004SeedDataTypes(
        List<ValidationDiagnostic> diagnostics,
        string entityName,
        Entities.NinjadogEntity entity)
    {
        if (entity.SeedData is null)
        {
            return;
        }

        for (var i = 0; i < entity.SeedData.Count; i++)
        {
            var row = entity.SeedData[i];
            foreach (var (fieldName, value) in row)
            {
                if (!entity.Properties.TryGetValue(fieldName, out var prop))
                {
                    continue;
                }

                if (!IsValueCompatibleWithType(value, prop.Type))
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "NINJ004",
                        $"Seed data value for '{fieldName}' in entity '{entityName}' at index {i} may not match type '{prop.Type}'.",
                        ValidationSeverity.Warning,
                        $"$.entities.{entityName}.seedData[{i}].{fieldName}"));
                }
            }
        }
    }

    private static void CheckNinj005NonBuiltInType(
        List<ValidationDiagnostic> diagnostics,
        string entityName,
        Entities.NinjadogEntity entity,
        NinjadogSettings settings)
    {
        foreach (var (propName, prop) in entity.Properties)
        {
            if (IsBuiltInType(prop.Type))
            {
                continue;
            }

            var hasEnum = settings.Enums is not null && settings.Enums.ContainsKey(prop.Type);
            var hasEntity = settings.Entities.ContainsKey(prop.Type);

            if (!hasEnum && !hasEntity)
            {
                diagnostics.Add(new ValidationDiagnostic(
                    "NINJ005",
                    $"Property '{propName}' in entity '{entityName}' uses type '{prop.Type}' which is not a built-in type and is not defined in the enums section.",
                    ValidationSeverity.Error,
                    $"$.entities.{entityName}.properties.{propName}.type"));
            }
        }
    }

    private static void CheckNinj006MinMaxLength(
        List<ValidationDiagnostic> diagnostics,
        string entityName,
        Entities.NinjadogEntity entity)
    {
        foreach (var (propName, prop) in entity.Properties)
        {
            if (prop.MinLength.HasValue && prop.MaxLength.HasValue && prop.MinLength > prop.MaxLength)
            {
                diagnostics.Add(new ValidationDiagnostic(
                    "NINJ006",
                    $"Property '{propName}' in entity '{entityName}' has minLength ({prop.MinLength}) greater than maxLength ({prop.MaxLength}).",
                    ValidationSeverity.Error,
                    $"$.entities.{entityName}.properties.{propName}"));
            }
        }
    }

    private static void CheckNinj007MinMax(
        List<ValidationDiagnostic> diagnostics,
        string entityName,
        Entities.NinjadogEntity entity)
    {
        foreach (var (propName, prop) in entity.Properties)
        {
            if (prop.Min.HasValue && prop.Max.HasValue && prop.Min > prop.Max)
            {
                diagnostics.Add(new ValidationDiagnostic(
                    "NINJ007",
                    $"Property '{propName}' in entity '{entityName}' has min ({prop.Min}) greater than max ({prop.Max}).",
                    ValidationSeverity.Error,
                    $"$.entities.{entityName}.properties.{propName}"));
            }
        }
    }

    private static void CheckNinj008PascalCase(
        List<ValidationDiagnostic> diagnostics,
        string entityName)
    {
        if (!PascalCaseRegex().IsMatch(entityName))
        {
            diagnostics.Add(new ValidationDiagnostic(
                "NINJ008",
                $"Entity name '{entityName}' should be PascalCase.",
                ValidationSeverity.Warning,
                $"$.entities.{entityName}"));
        }
    }

    private static void CheckNinj009EnumUniqueness(
        List<ValidationDiagnostic> diagnostics,
        NinjadogSettings settings)
    {
        if (settings.Enums is null)
        {
            return;
        }

        foreach (var (enumName, values) in settings.Enums)
        {
            var seen = new HashSet<string>();
            foreach (var value in values)
            {
                if (!seen.Add(value))
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "NINJ009",
                        $"Enum '{enumName}' contains duplicate value '{value}'.",
                        ValidationSeverity.Warning,
                        $"$.enums.{enumName}"));
                }
            }
        }
    }

    private static void CheckNinj010SeedDataKeyUniqueness(
        List<ValidationDiagnostic> diagnostics,
        string entityName,
        Entities.NinjadogEntity entity)
    {
        if (entity.SeedData is null)
        {
            return;
        }

        var keyPropName = entity.Properties
            .FirstOrDefault(p => p.Value.IsKey).Key;

        if (keyPropName is null)
        {
            return;
        }

        var seenKeys = new HashSet<string>();
        for (var i = 0; i < entity.SeedData.Count; i++)
        {
            var row = entity.SeedData[i];
            if (row.TryGetValue(keyPropName, out var keyValue))
            {
                var keyStr = keyValue.ToString()!;
                if (!seenKeys.Add(keyStr))
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "NINJ010",
                        $"Seed data in entity '{entityName}' has duplicate key value '{keyStr}' at index {i}.",
                        ValidationSeverity.Warning,
                        $"$.entities.{entityName}.seedData[{i}].{keyPropName}"));
                }
            }
        }
    }

    private static bool IsBuiltInType(string type)
    {
        return _builtInTypes.Contains(type) || type.StartsWith("List<", StringComparison.Ordinal);
    }

    private static bool IsValueCompatibleWithType(object value, string type)
    {
        return type.ToLowerInvariant() switch
        {
            "string" => value is string,
            "int" or "int32" => value is int or long,
            "long" or "int64" => value is int or long,
            "float" or "single" => value is int or long or float or double or decimal,
            "double" => value is int or long or float or double or decimal,
            "decimal" => value is int or long or float or double or decimal,
            "bool" or "boolean" => value is bool,
            "guid" => value is string s && Guid.TryParse(s, out _),
            _ => true // Unknown types pass (enum values are strings)
        };
    }

    [GeneratedRegex(@"^[A-Z][a-zA-Z0-9]*$")]
    private static partial Regex PascalCaseRegex();
}
