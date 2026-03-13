using System.Text.Json;
using System.Text.Json.Serialization;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Entities.Properties;

namespace Ninjadog.Settings.Parsing;

/// <summary>
/// Parses the "entities" section of a ninjadog.json file into a <see cref="NinjadogLoadedEntities"/>.
/// </summary>
internal static class NinjadogEntitiesParser
{
    private static readonly JsonSerializerOptions _deserializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal static NinjadogLoadedEntities Parse(JsonElement root, string? basePath)
    {
        var entities = new NinjadogLoadedEntities();

        if (!root.TryGetOptionalObject("entities", out var entitiesElement))
        {
            return entities;
        }

        foreach (var entityProp in entitiesElement.EnumerateObject())
        {
            var entityName = entityProp.Name;
            var properties = ParseProperties(entityProp.Value);
            var relationships = ParseRelationships(entityProp.Value);
            var seedData = ParseSeedData(entityProp.Value, basePath);

            entities.Add(entityName, new NinjadogEntity(properties, relationships, seedData));
        }

        return entities;
    }

    private static NinjadogEntityProperties ParseProperties(JsonElement entityElement)
    {
        var properties = new NinjadogEntityProperties();

        if (!entityElement.TryGetOptionalObject("properties", out var propsElement))
        {
            return properties;
        }

        foreach (var prop in propsElement.EnumerateObject())
        {
            properties.Add(prop.Name, new NinjadogEntityProperty(
                Type: prop.Value.GetRequiredString("type"),
                IsKey: prop.Value.GetOptionalBoolean("isKey"),
                Required: prop.Value.GetOptionalBoolean("required"),
                MaxLength: prop.Value.GetOptionalInt32("maxLength"),
                MinLength: prop.Value.GetOptionalInt32("minLength"),
                Min: prop.Value.GetOptionalInt32("min"),
                Max: prop.Value.GetOptionalInt32("max"),
                Pattern: prop.Value.GetOptionalString("pattern")));
        }

        return properties;
    }

    private static NinjadogEntityRelationships? ParseRelationships(JsonElement entityElement)
    {
        return !entityElement.TryGetOptionalObject("relationships", out var relsElement)
            ? null
            : JsonSerializer.Deserialize<NinjadogEntityRelationships>(
                relsElement.GetRawText(), _deserializeOptions);
    }

    private static List<Dictionary<string, object>>? ParseSeedData(JsonElement entityElement, string? basePath)
    {
        return !entityElement.TryGetOptionalProperty("seedData", out var seedProperty)
            ? null
            : seedProperty.ValueKind switch
            {
                JsonValueKind.String => CsvSeedDataParser.ParseCsvFile(seedProperty.GetString()!, basePath),
                JsonValueKind.Array => ParseSeedDataArray(seedProperty),
                _ => throw new JsonException("Expected 'seedData' to be a JSON array or a string path to a CSV file.")
            };
    }

    private static List<Dictionary<string, object>> ParseSeedDataArray(JsonElement seedArray)
    {
        var seedData = new List<Dictionary<string, object>>();

        foreach (var seedItem in seedArray.EnumerateArray())
        {
            var row = new Dictionary<string, object>();
            foreach (var field in seedItem.EnumerateObject())
            {
                row[field.Name] = field.Value.ValueKind switch
                {
                    JsonValueKind.String => field.Value.GetString()!,
                    JsonValueKind.Number => field.Value.TryGetInt32(out var intVal) ? intVal : field.Value.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Array or JsonValueKind.Null or _ => field.Value.GetRawText(),
                };
            }

            seedData.Add(row);
        }

        return seedData;
    }
}
