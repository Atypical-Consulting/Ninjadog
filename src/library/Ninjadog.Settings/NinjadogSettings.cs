// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Entities.Properties;

namespace Ninjadog.Settings;

/// <summary>
/// Represents the settings for a Ninjadog Engine instance, encapsulating all necessary configurations.
/// This record includes both application-level configurations and entity definitions,
/// enabling tailored behavior of the Ninjadog Engine based on these settings.
/// </summary>
/// <param name="Config">The general configuration settings for the Ninjadog Engine.</param>
/// <param name="Entities">The collection of entities that the Ninjadog Engine will use for template generation.</param>
public abstract record NinjadogSettings(
    NinjadogConfiguration Config,
    NinjadogEntities Entities,
    Dictionary<string, List<string>>? Enums = null)
{
    private static readonly JsonSerializerOptions _deserializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the record to JSON using the JsonSerializationContext.
    /// </summary>
    /// <returns>A JSON string representing the NinjadogSettings record.</returns>
    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this, JsonSerializationContext.Default.NinjadogSettings);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="NinjadogSettings"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="NinjadogSettings"/> instance loaded from the JSON string.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static NinjadogSettings FromJsonString(string json)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var configElement = root.GetProperty("config");
        var name = configElement.GetProperty("name").GetString()!;

        NinjadogCorsConfiguration? cors = null;
        if (configElement.TryGetProperty("cors", out var corsElement))
        {
            var origins = corsElement.TryGetProperty("origins", out var originsEl)
                ? originsEl.EnumerateArray().Select(e => e.GetString()!).ToArray()
                : [];
            var methods = corsElement.TryGetProperty("methods", out var methodsEl)
                ? methodsEl.EnumerateArray().Select(e => e.GetString()!).ToArray()
                : null;
            var headers = corsElement.TryGetProperty("headers", out var headersEl)
                ? headersEl.EnumerateArray().Select(e => e.GetString()!).ToArray()
                : null;
            cors = new NinjadogCorsConfiguration(origins, methods, headers);
        }

        var config = new NinjadogLoadedConfiguration(
            Name: name,
            Version: configElement.GetProperty("version").GetString()!,
            Description: configElement.GetProperty("description").GetString()!,
            RootNamespace: configElement.GetProperty("rootNamespace").GetString()!,
            OutputPath: configElement.TryGetProperty("outputPath", out var op) ? op.GetString()! : $"src/applications/{name}",
            SaveGeneratedFiles: configElement.TryGetProperty("saveGeneratedFiles", out var sgf) && sgf.GetBoolean(),
            Cors: cors);

        var entities = new NinjadogLoadedEntities();

        if (root.TryGetProperty("entities", out var entitiesElement))
        {
            foreach (var entityProp in entitiesElement.EnumerateObject())
            {
                var entityName = entityProp.Name;
                var properties = new NinjadogEntityProperties();

                if (entityProp.Value.TryGetProperty("properties", out var propsElement))
                {
                    foreach (var prop in propsElement.EnumerateObject())
                    {
                        var propName = prop.Name;
                        var type = prop.Value.GetProperty("type").GetString()!;
                        var isKey = prop.Value.TryGetProperty("isKey", out var ik) && ik.GetBoolean();
                        var required = prop.Value.TryGetProperty("required", out var req) && req.GetBoolean();
                        int? maxLength = prop.Value.TryGetProperty("maxLength", out var ml) ? ml.GetInt32() : null;
                        int? minLength = prop.Value.TryGetProperty("minLength", out var mnl) ? mnl.GetInt32() : null;
                        int? min = prop.Value.TryGetProperty("min", out var mn) ? mn.GetInt32() : null;
                        int? max = prop.Value.TryGetProperty("max", out var mx) ? mx.GetInt32() : null;
                        var pattern = prop.Value.TryGetProperty("pattern", out var pat) ? pat.GetString() : null;
                        properties.Add(propName, new NinjadogEntityProperty(type, isKey, required, maxLength, minLength, min, max, pattern));
                    }
                }

                NinjadogEntityRelationships? relationships = null;
                if (entityProp.Value.TryGetProperty("relationships", out var relsElement))
                {
                    relationships = JsonSerializer.Deserialize<NinjadogEntityRelationships>(
                        relsElement.GetRawText(), _deserializeOptions);
                }

                List<Dictionary<string, object>>? seedData = null;
                if (entityProp.Value.TryGetProperty("seedData", out var seedElement))
                {
                    seedData = [];
                    foreach (var seedItem in seedElement.EnumerateArray())
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
                                _ => field.Value.GetRawText()
                            };
                        }

                        seedData.Add(row);
                    }
                }

                entities.Add(entityName, new NinjadogEntity(properties, relationships, seedData));
            }
        }

        Dictionary<string, List<string>>? enums = null;
        if (root.TryGetProperty("enums", out var enumsElement))
        {
            enums = new Dictionary<string, List<string>>();
            foreach (var enumProp in enumsElement.EnumerateObject())
            {
                var values = enumProp.Value.EnumerateArray()
                    .Select(e => e.GetString()!)
                    .ToList();
                enums[enumProp.Name] = values;
            }
        }

        return new NinjadogLoadedSettings(config, entities, enums);
    }
}
