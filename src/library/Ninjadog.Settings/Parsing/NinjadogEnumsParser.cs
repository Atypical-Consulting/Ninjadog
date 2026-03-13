using System.Text.Json;

namespace Ninjadog.Settings.Parsing;

/// <summary>
/// Parses the "enums" section of a ninjadog.json file.
/// </summary>
internal static class NinjadogEnumsParser
{
    internal static Dictionary<string, List<string>>? Parse(JsonElement root)
    {
        if (!root.TryGetOptionalObject("enums", out var enumsElement))
        {
            return null;
        }

        var enums = new Dictionary<string, List<string>>();

        foreach (var enumProp in enumsElement.EnumerateObject())
        {
            var values = enumProp.Value.EnumerateArray()
                .Select(e => e.GetString()!)
                .ToList();
            enums[enumProp.Name] = values;
        }

        return enums;
    }
}
