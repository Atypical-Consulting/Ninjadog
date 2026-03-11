// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Json.Schema;

namespace Ninjadog.Settings.Schema;

/// <summary>
/// Provides access to the embedded Ninjadog JSON Schema.
/// </summary>
public static class SchemaProvider
{
    private const string SchemaResourceName = "Ninjadog.Settings.Schema.ninjadog.schema.json";
    private static readonly Lazy<JsonSchema> _schema = new(LoadSchema);

    /// <summary>
    /// Gets the Ninjadog JSON Schema instance.
    /// </summary>
    public static JsonSchema Schema => _schema.Value;

    /// <summary>
    /// Gets the raw JSON Schema text as a string.
    /// </summary>
    /// <returns>The JSON Schema content as a string.</returns>
    public static string GetSchemaText()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(SchemaResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{SchemaResourceName}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static JsonSchema LoadSchema()
    {
        var schemaText = GetSchemaText();
        return JsonSchema.FromText(schemaText);
    }
}
