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
    /// <param name="basePath">Optional base directory path for resolving relative file references (e.g., CSV seed data files).</param>
    /// <returns>A <see cref="NinjadogSettings"/> instance loaded from the JSON string.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static NinjadogSettings FromJsonString(string json, string? basePath = null)
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

        NinjadogVersioningConfiguration? versioning = null;
        if (TryGetOptionalObject(configElement, "versioning", out var versioningElement))
        {
            var strategy = GetOptionalString(versioningElement, "strategy") ?? NinjadogVersioningConfiguration.UrlPathStrategy;
            var defaultVersion = GetOptionalInt32(versioningElement, "defaultVersion") ?? 1;
            var prefix = GetOptionalString(versioningElement, "prefix") ?? "v";
            var headerName = GetOptionalString(versioningElement, "headerName") ?? "X-Api-Version";
            versioning = new NinjadogVersioningConfiguration(strategy, defaultVersion, prefix, headerName);
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

        NinjadogRateLimitConfiguration? rateLimit = null;
        if (TryGetOptionalObject(configElement, "rateLimit", out var rateLimitElement))
        {
            var permitLimit = GetOptionalInt32(rateLimitElement, "permitLimit") ?? 100;
            var windowSeconds = GetOptionalInt32(rateLimitElement, "windowSeconds") ?? 60;
            var segmentsPerWindow = GetOptionalInt32(rateLimitElement, "segmentsPerWindow") ?? 6;
            rateLimit = new NinjadogRateLimitConfiguration(permitLimit, windowSeconds, segmentsPerWindow);
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
            Auth: auth,
            RateLimit: rateLimit,
            Versioning: versioning);

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
                if (TryGetOptionalProperty(entityProp.Value, "seedData", out var seedProperty))
                {
                    if (seedProperty.ValueKind == JsonValueKind.String)
                    {
                        var csvPath = seedProperty.GetString()!;
                        seedData = ParseCsvSeedData(csvPath, basePath);
                    }
                    else if (seedProperty.ValueKind == JsonValueKind.Array)
                    {
                        seedData = [];
                        foreach (var seedItem in seedProperty.EnumerateArray())
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
                    else
                    {
                        throw new JsonException("Expected 'seedData' to be a JSON array or a string path to a CSV file.");
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

    private static bool TryGetOptionalProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        property = default;

        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propertyName, out property)
            && property.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;
    }

    private static List<Dictionary<string, object>> ParseCsvSeedData(string csvPath, string? basePath)
    {
        var resolvedPath = basePath is not null
            ? Path.Combine(basePath, csvPath)
            : csvPath;

        string[] lines;
        try
        {
            lines = File.ReadAllLines(resolvedPath);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new InvalidOperationException($"Seed data CSV file not found: '{resolvedPath}'.", ex);
        }

        if (lines.Length == 0)
        {
            throw new InvalidOperationException($"Seed data CSV file '{csvPath}' is empty.");
        }

        var headers = ParseCsvLine(lines[0]);
        var seedData = new List<Dictionary<string, object>>();

        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);
            if (values.Length != headers.Length)
            {
                throw new InvalidOperationException($"Seed data CSV file '{csvPath}' row {i + 1} has {values.Length} fields but header has {headers.Length}.");
            }

            var row = new Dictionary<string, object>();
            for (var j = 0; j < headers.Length; j++)
            {
                row[headers[j]] = ParseCsvValue(values[j]);
            }

            seedData.Add(row);
        }

        return seedData;
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString().Trim());

        return [.. fields];
    }

    private static object ParseCsvValue(string value) =>
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ? true :
        string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ? false :
        int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var intVal) ? intVal :
        decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var decVal) ? (object)decVal :
        value;
}
