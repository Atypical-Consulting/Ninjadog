using System.Text.Json;
using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Parsing;

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
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var config = NinjadogConfigParser.Parse(root.GetRequiredObject("config"));
        var entities = NinjadogEntitiesParser.Parse(root, basePath);
        var enums = NinjadogEnumsParser.Parse(root);

        return new NinjadogLoadedSettings(config, entities, enums);
    }
}
