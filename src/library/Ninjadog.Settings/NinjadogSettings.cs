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
/// <param name="Enums">The optional enum definitions mapping enum names to their values.</param>
public abstract record NinjadogSettings(
    NinjadogConfiguration Config,
    NinjadogEntities Entities,
    Dictionary<string, List<string>>? Enums = null)
{
    private const string ConfigPropertyName = "config";
    private const string EntitiesPropertyName = "entities";
    private const string EnumsPropertyName = "enums";
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

        var configElement = GetRequiredObject(root, ConfigPropertyName);
        var name = GetRequiredString(configElement, "name");

        NinjadogCorsConfiguration? cors = null;
        if (TryGetOptionalObject(configElement, "cors", out var corsElement))
        {
            var origins = GetOptionalStringArray(corsElement, "origins") ?? [];
            var methods = GetOptionalStringArray(corsElement, "methods");
            var headers = GetOptionalStringArray(corsElement, "headers");
            cors = new NinjadogCorsConfiguration(origins, methods, headers);
        }

        var softDelete = false;
        var auditing = false;
        var aot = false;
        if (TryGetOptionalObject(configElement, "features", out var featuresElement))
        {
            softDelete = GetOptionalBoolean(featuresElement, "softDelete");
            auditing = GetOptionalBoolean(featuresElement, "auditing");
            aot = GetOptionalBoolean(featuresElement, "aot");
        }

        var databaseProvider = "sqlite";
        if (TryGetOptionalObject(configElement, "database", out var dbElement))
        {
            var provider = GetOptionalString(dbElement, "provider");
            if (provider is not null)
            {
                databaseProvider = provider;
            }
        }

        NinjadogAuthConfiguration? auth = null;
        if (TryGetOptionalObject(configElement, "auth", out var authElement))
        {
            var authProvider = GetOptionalString(authElement, "provider") ?? "jwt";
            var issuer = GetOptionalString(authElement, "issuer") ?? "https://localhost";
            var audience = GetOptionalString(authElement, "audience") ?? "api";
            var tokenExpirationMinutes = GetOptionalInt32(authElement, "tokenExpirationMinutes") ?? 60;
            var roles = GetOptionalStringArray(authElement, "roles");
            var generateLoginEndpoint = !TryGetOptionalProperty(authElement, "generateLoginEndpoint", out _) || GetOptionalBoolean(authElement, "generateLoginEndpoint");
            var generateRegisterEndpoint = !TryGetOptionalProperty(authElement, "generateRegisterEndpoint", out _) || GetOptionalBoolean(authElement, "generateRegisterEndpoint");
            auth = new NinjadogAuthConfiguration(authProvider, issuer, audience, tokenExpirationMinutes, roles, generateLoginEndpoint, generateRegisterEndpoint);
        }

        var config = new NinjadogLoadedConfiguration(
            Name: name,
            Version: GetRequiredString(configElement, "version"),
            Description: GetRequiredString(configElement, "description"),
            RootNamespace: GetRequiredString(configElement, "rootNamespace"),
            OutputPath: GetOptionalString(configElement, "outputPath") ?? ".",
            SaveGeneratedFiles: GetOptionalBoolean(configElement, "saveGeneratedFiles"),
            Cors: cors,
            SoftDelete: softDelete,
            Auditing: auditing,
            DatabaseProvider: databaseProvider,
            Aot: aot,
            Auth: auth);

        var entities = new NinjadogLoadedEntities();

        if (TryGetOptionalObject(root, EntitiesPropertyName, out var entitiesElement))
        {
            foreach (var entityProp in entitiesElement.EnumerateObject())
            {
                var entityName = entityProp.Name;
                var properties = new NinjadogEntityProperties();

                if (TryGetOptionalObject(entityProp.Value, "properties", out var propsElement))
                {
                    foreach (var prop in propsElement.EnumerateObject())
                    {
                        var propName = prop.Name;
                        var type = GetRequiredString(prop.Value, "type");
                        var isKey = GetOptionalBoolean(prop.Value, "isKey");
                        var required = GetOptionalBoolean(prop.Value, "required");
                        var maxLength = GetOptionalInt32(prop.Value, "maxLength");
                        var minLength = GetOptionalInt32(prop.Value, "minLength");
                        var min = GetOptionalInt32(prop.Value, "min");
                        var max = GetOptionalInt32(prop.Value, "max");
                        var pattern = GetOptionalString(prop.Value, "pattern");
                        properties.Add(propName, new NinjadogEntityProperty(type, isKey, required, maxLength, minLength, min, max, pattern));
                    }
                }

                NinjadogEntityRelationships? relationships = null;
                if (TryGetOptionalObject(entityProp.Value, "relationships", out var relsElement))
                {
                    relationships = JsonSerializer.Deserialize<NinjadogEntityRelationships>(
                        relsElement.GetRawText(), _deserializeOptions);
                }

                List<Dictionary<string, object>>? seedData = null;
                if (TryGetOptionalArray(entityProp.Value, "seedData", out var seedElement))
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
                                JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Array or JsonValueKind.Null or _ => field.Value.GetRawText(),
                            };
                        }

                        seedData.Add(row);
                    }
                }

                entities.Add(entityName, new NinjadogEntity(properties, relationships, seedData));
            }
        }

        Dictionary<string, List<string>>? enums = null;
        if (TryGetOptionalObject(root, EnumsPropertyName, out var enumsElement))
        {
            enums = [];
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

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        return !TryGetOptionalProperty(element, propertyName, out var property) || property.ValueKind != JsonValueKind.String
            ? throw new JsonException($"Expected '{propertyName}' to be a JSON string.")
            : property.GetString()!;
    }

    private static string? GetOptionalString(JsonElement element, string propertyName)
    {
        return !TryGetOptionalProperty(element, propertyName, out var property)
            ? null
            : property.ValueKind != JsonValueKind.String
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON string.")
                : property.GetString();
    }

    private static bool GetOptionalBoolean(JsonElement element, string propertyName)
    {
        return TryGetOptionalProperty(element, propertyName, out var property)
            && (property.ValueKind is not JsonValueKind.True and not JsonValueKind.False
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON boolean.")
                : property.GetBoolean());
    }

    private static int? GetOptionalInt32(JsonElement element, string propertyName)
    {
        return !TryGetOptionalProperty(element, propertyName, out var property)
            ? null
            : property.ValueKind != JsonValueKind.Number
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON number.")
                : property.GetInt32();
    }

    private static string[]? GetOptionalStringArray(JsonElement element, string propertyName)
    {
        return !TryGetOptionalProperty(element, propertyName, out var property)
            ? null
            : property.ValueKind != JsonValueKind.Array
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON array.")
                : [.. property.EnumerateArray().Select(item => item.GetString()!)];
    }

    private static JsonElement GetRequiredObject(JsonElement element, string propertyName)
    {
        return !TryGetOptionalObject(element, propertyName, out var property)
            ? throw new JsonException($"Expected '{propertyName}' to be a JSON object.")
            : property;
    }

    private static bool TryGetOptionalObject(JsonElement element, string propertyName, out JsonElement property)
    {
        return TryGetOptionalProperty(element, propertyName, out property)
            && (property.ValueKind != JsonValueKind.Object
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON object.")
                : true);
    }

    private static bool TryGetOptionalArray(JsonElement element, string propertyName, out JsonElement property)
    {
        return TryGetOptionalProperty(element, propertyName, out property)
            && (property.ValueKind != JsonValueKind.Array
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON array.")
                : true);
    }

    private static bool TryGetOptionalProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        property = default;

        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propertyName, out property)
            && property.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;
    }
}
