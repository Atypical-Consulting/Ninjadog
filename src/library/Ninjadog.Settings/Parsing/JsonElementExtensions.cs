using System.Text.Json;

namespace Ninjadog.Settings.Parsing;

/// <summary>
/// Helper extension methods for extracting typed values from <see cref="JsonElement"/>.
/// </summary>
internal static class JsonElementExtensions
{
    internal static string GetRequiredString(this JsonElement element, string propertyName)
    {
        return !element.TryGetOptionalProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String
            ? throw new JsonException($"Expected '{propertyName}' to be a JSON string.")
            : property.GetString()!;
    }

    internal static string? GetOptionalString(this JsonElement element, string propertyName)
    {
        return !element.TryGetOptionalProperty(propertyName, out var property)
            ? null
            : property.ValueKind != JsonValueKind.String
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON string.")
                : property.GetString();
    }

    internal static bool GetOptionalBoolean(this JsonElement element, string propertyName)
    {
        return element.TryGetOptionalProperty(propertyName, out var property)
            && (property.ValueKind is not JsonValueKind.True and not JsonValueKind.False
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON boolean.")
                : property.GetBoolean());
    }

    internal static int? GetOptionalInt32(this JsonElement element, string propertyName)
    {
        return !element.TryGetOptionalProperty(propertyName, out var property)
            ? null
            : property.ValueKind != JsonValueKind.Number
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON number.")
                : property.GetInt32();
    }

    internal static string[]? GetOptionalStringArray(this JsonElement element, string propertyName)
    {
        return !element.TryGetOptionalProperty(propertyName, out var property)
            ? null
            : property.ValueKind != JsonValueKind.Array
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON array.")
                : [.. property.EnumerateArray().Select(item => item.GetString()!)];
    }

    internal static JsonElement GetRequiredObject(this JsonElement element, string propertyName)
    {
        return !element.TryGetOptionalObject(propertyName, out var property)
            ? throw new JsonException($"Expected '{propertyName}' to be a JSON object.")
            : property;
    }

    internal static bool TryGetOptionalObject(this JsonElement element, string propertyName, out JsonElement property)
    {
        return element.TryGetOptionalProperty(propertyName, out property)
            && (property.ValueKind != JsonValueKind.Object
                ? throw new JsonException($"Expected '{propertyName}' to be a JSON object.")
                : true);
    }

    internal static bool TryGetOptionalProperty(this JsonElement element, string propertyName, out JsonElement property)
    {
        property = default;

        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propertyName, out property)
            && property.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;
    }
}
