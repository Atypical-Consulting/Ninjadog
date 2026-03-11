// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Extensions.Entities;

/// <summary>
/// Represents a Ninjadog entity along with its key.
/// This record extends <see cref="NinjadogEntity"/> by associating it with a specific key,
/// enabling more specific template generation based on the key-value relationship.
/// </summary>
/// <param name="Key">The key of the entity.</param>
/// <param name="Properties">The properties of the entity.</param>
/// <param name="Relationships">The relationships between this entity and other entities.</param>
public sealed record NinjadogEntityWithKey(
    string Key,
    NinjadogEntityProperties Properties,
    NinjadogEntityRelationships? Relationships,
    List<Dictionary<string, object>>? SeedData = null)
    : NinjadogEntity(Properties, Relationships, SeedData)
{
    /// <summary>
    /// Gets the string tokens generated from the entity key.
    /// </summary>
    public StringTokens StringTokens => new(Key);

    /// <summary>
    /// Generates the member properties for the entity.
    /// This method produces C# code for each property within the entity, formatted as member definitions.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the C# code for all member properties of the entity.</returns>
    public string GenerateMemberProperties()
    {
        var properties = Properties
            .FromKeys()
            .Select(p => p.GenerateMemberProperty())
            .Aggregate((x, y) => $"{x}\n{y}");

        return properties;
    }
}
