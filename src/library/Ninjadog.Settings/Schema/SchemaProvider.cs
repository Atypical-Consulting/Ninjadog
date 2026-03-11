// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

namespace Ninjadog.Settings.Schema;

/// <summary>
/// Provides access to the embedded ninjadog JSON schema.
/// </summary>
public static class SchemaProvider
{
    private const string ResourceName = "Ninjadog.Settings.Schema.ninjadog.schema.json";

    private static readonly Lazy<string> _schemaJson = new(LoadSchemaJsonCore);

    /// <summary>
    /// Loads the embedded ninjadog.schema.json as a string. The result is cached after the first call.
    /// </summary>
    /// <returns>The JSON schema content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the embedded resource cannot be found.</exception>
    public static string LoadSchemaJson() => _schemaJson.Value;

    private static string LoadSchemaJsonCore()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{ResourceName}' not found in assembly '{assembly.FullName}'.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
