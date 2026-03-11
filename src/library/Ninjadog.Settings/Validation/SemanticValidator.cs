// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Ninjadog.Settings.Validation;

/// <summary>
/// Performs semantic validation on parsed NinjadogSettings, producing diagnostics with NINJ0xx codes.
/// </summary>
public static class SemanticValidator
{
    private static readonly HashSet<string> _builtInTypes = new(StringComparer.Ordinal)
    {
        "string", "String",
        "int", "Int32",
        "long", "Int64",
        "float", "Single",
        "double", "Double",
        "decimal", "Decimal",
        "bool", "Boolean",
        "Guid",
        "DateTime", "DateTimeOffset",
        "DateOnly", "TimeOnly",
        "byte[]"
    };

    private static readonly Regex _pascalCaseRegex = new(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

    /// <summary>
    /// Validates the given NinjadogSettings and returns a list of semantic diagnostics.
    /// </summary>
    /// <param name="settings">The parsed settings to validate.</param>
    /// <returns>A list of validation diagnostics.</returns>
    public static List<ValidationDiagnostic> Validate(NinjadogSettings settings)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        var entityNames = new HashSet<string>(settings.Entities.Keys, StringComparer.Ordinal);

        foreach (var (entityName, entity) in settings.Entities)
        {
            var entityPath = $"entities.{entityName}";

            // NINJ001: Each entity must have exactly one isKey: true property
            CheckExactlyOneKey(entityName, entity, entityPath, diagnostics);

            // NINJ002: Relationship relatedEntity must reference existing entity
            CheckRelationshipReferences(entity, entityPath, entityNames, diagnostics);

            // NINJ003: Seed data field names must match entity property names
            CheckSeedDataFieldNames(entity, entityPath, diagnostics);

            // NINJ004: Seed data values should match property types
            CheckSeedDataTypes(entity, entityPath, diagnostics);

            // NINJ005: Non-built-in property type must exist in enums section
            CheckPropertyTypesExist(entity, entityPath, settings.Enums, diagnostics);

            // NINJ006: minLength must not exceed maxLength
            CheckMinMaxLength(entity, entityPath, diagnostics);

            // NINJ007: min must not exceed max
            CheckMinMax(entity, entityPath, diagnostics);

            // NINJ008: Entity names should be PascalCase
            CheckPascalCase(entityName, entityPath, diagnostics);

            // NINJ010: Seed data key values should be unique per entity
            CheckSeedDataKeyUniqueness(entity, entityPath, diagnostics);
        }

        // NINJ009: Enum values should be unique within an enum
        CheckEnumValueUniqueness(settings.Enums, diagnostics);

        return diagnostics;
    }

    private static void CheckExactlyOneKey(
        string entityName,
        Entities.NinjadogEntity entity,
        string entityPath,
        List<ValidationDiagnostic> diagnostics)
    {
        var keyCount = entity.Properties.Count(p => p.Value.IsKey);
        if (keyCount == 0)
        {
            diagnostics.Add(new ValidationDiagnostic(
                "NINJ001",
                $"Entity '{entityName}' has no key property. Exactly one property must have isKey: true.",
                ValidationSeverity.Error,
                entityPath));
        }
        else if (keyCount > 1)
        {
            diagnostics.Add(new ValidationDiagnostic(
                "NINJ001",
                $"Entity '{entityName}' has {keyCount} key properties. Exactly one property must have isKey: true.",
                ValidationSeverity.Error,
                entityPath));
        }
    }

    private static void CheckRelationshipReferences(
        Entities.NinjadogEntity entity,
        string entityPath,
        HashSet<string> entityNames,
        List<ValidationDiagnostic> diagnostics)
    {
        if (entity.Relationships is null)
        {
            return;
        }

        foreach (var (relName, relationship) in entity.Relationships)
        {
            if (!entityNames.Contains(relationship.RelatedEntity))
            {
                diagnostics.Add(new ValidationDiagnostic(
                    "NINJ002",
                    $"Relationship '{relName}' references entity '{relationship.RelatedEntity}' which does not exist.",
                    ValidationSeverity.Error,
                    $"{entityPath}.relationships.{relName}"));
            }
        }
    }

    private static void CheckSeedDataFieldNames(
        Entities.NinjadogEntity entity,
        string entityPath,
        List<ValidationDiagnostic> diagnostics)
    {
        if (entity.SeedData is null)
        {
            return;
        }

        var propertyNames = new HashSet<string>(entity.Properties.Keys, StringComparer.Ordinal);

        for (var i = 0; i < entity.SeedData.Count; i++)
        {
            foreach (var fieldName in entity.SeedData[i].Keys)
            {
                if (!propertyNames.Contains(fieldName))
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "NINJ003",
                        $"Seed data field '{fieldName}' at index {i} does not match any property on the entity.",
                        ValidationSeverity.Error,
                        $"{entityPath}.seedData[{i}]"));
                }
            }
        }
    }

    private static void CheckSeedDataTypes(
        Entities.NinjadogEntity entity,
        string entityPath,
        List<ValidationDiagnostic> diagnostics)
    {
        if (entity.SeedData is null)
        {
            return;
        }

        for (var i = 0; i < entity.SeedData.Count; i++)
        {
            foreach (var (fieldName, value) in entity.SeedData[i])
            {
                if (!entity.Properties.TryGetValue(fieldName, out var prop))
                {
                    continue;
                }

                if (!IsValueCompatibleWithType(value, prop.Type))
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "NINJ004",
                        $"Seed data field '{fieldName}' at index {i} has value '{value}' which may not match type '{prop.Type}'.",
                        ValidationSeverity.Warning,
                        $"{entityPath}.seedData[{i}].{fieldName}"));
                }
            }
        }
    }

    private static void CheckPropertyTypesExist(
        Entities.NinjadogEntity entity,
        string entityPath,
        Dictionary<string, List<string>>? enums,
        List<ValidationDiagnostic> diagnostics)
    {
        foreach (var (propName, prop) in entity.Properties)
        {
            if (IsBuiltInType(prop.Type))
            {
                continue;
            }

            if (enums is null || !enums.ContainsKey(prop.Type))
            {
                diagnostics.Add(new ValidationDiagnostic(
                    "NINJ005",
                    $"Property '{propName}' has type '{prop.Type}' which is not a built-in type and is not defined in the enums section.",
                    ValidationSeverity.Error,
                    $"{entityPath}.properties.{propName}"));
            }
        }
    }

    private static void CheckMinMaxLength(
        Entities.NinjadogEntity entity,
        string entityPath,
        List<ValidationDiagnostic> diagnostics)
    {
        foreach (var (propName, prop) in entity.Properties)
        {
            if (prop.MinLength.HasValue && prop.MaxLength.HasValue && prop.MinLength.Value > prop.MaxLength.Value)
            {
                diagnostics.Add(new ValidationDiagnostic(
                    "NINJ006",
                    $"Property '{propName}' has minLength ({prop.MinLength}) greater than maxLength ({prop.MaxLength}).",
                    ValidationSeverity.Error,
                    $"{entityPath}.properties.{propName}"));
            }
        }
    }

    private static void CheckMinMax(
        Entities.NinjadogEntity entity,
        string entityPath,
        List<ValidationDiagnostic> diagnostics)
    {
        foreach (var (propName, prop) in entity.Properties)
        {
            if (prop.Min.HasValue && prop.Max.HasValue && prop.Min.Value > prop.Max.Value)
            {
                diagnostics.Add(new ValidationDiagnostic(
                    "NINJ007",
                    $"Property '{propName}' has min ({prop.Min}) greater than max ({prop.Max}).",
                    ValidationSeverity.Error,
                    $"{entityPath}.properties.{propName}"));
            }
        }
    }

    private static void CheckPascalCase(
        string entityName,
        string entityPath,
        List<ValidationDiagnostic> diagnostics)
    {
        if (!_pascalCaseRegex.IsMatch(entityName))
        {
            diagnostics.Add(new ValidationDiagnostic(
                "NINJ008",
                $"Entity name '{entityName}' is not PascalCase.",
                ValidationSeverity.Warning,
                entityPath));
        }
    }

    private static void CheckEnumValueUniqueness(
        Dictionary<string, List<string>>? enums,
        List<ValidationDiagnostic> diagnostics)
    {
        if (enums is null)
        {
            return;
        }

        foreach (var (enumName, values) in enums)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (!seen.Add(value))
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "NINJ009",
                        $"Enum '{enumName}' has duplicate value '{value}'.",
                        ValidationSeverity.Warning,
                        $"enums.{enumName}"));
                }
            }
        }
    }

    private static void CheckSeedDataKeyUniqueness(
        Entities.NinjadogEntity entity,
        string entityPath,
        List<ValidationDiagnostic> diagnostics)
    {
        if (entity.SeedData is null)
        {
            return;
        }

        var keyProp = entity.Properties.FirstOrDefault(p => p.Value.IsKey);
        if (keyProp.Key is null)
        {
            return;
        }

        var keyName = keyProp.Key;
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < entity.SeedData.Count; i++)
        {
            if (entity.SeedData[i].TryGetValue(keyName, out var keyValue))
            {
                var keyStr = keyValue?.ToString() ?? string.Empty;
                if (!seen.Add(keyStr))
                {
                    diagnostics.Add(new ValidationDiagnostic(
                        "NINJ010",
                        $"Seed data has duplicate key value '{keyStr}' for key property '{keyName}'.",
                        ValidationSeverity.Warning,
                        $"{entityPath}.seedData[{i}]"));
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
        return type switch
        {
            "int" or "Int32" or "long" or "Int64" => value is int or long or decimal,
            "float" or "Single" or "double" or "Double" or "decimal" or "Decimal" => value is int or long or float or double or decimal,
            "bool" or "Boolean" => value is bool,
            "string" or "String" or "Guid" or "DateTime" or "DateTimeOffset" or "DateOnly" or "TimeOnly" => value is string,
            _ => true // For enums and unknown types, accept any value
        };
    }
}
