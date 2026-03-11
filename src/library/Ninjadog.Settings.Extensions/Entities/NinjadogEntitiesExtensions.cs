// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Settings.Extensions.Entities;

/// <summary>
/// Provides extension methods for <see cref="NinjadogEntities"/> collections.
/// These methods extend the functionality of <see cref="NinjadogEntities"/>, offering additional utilities
/// such as conversion and manipulation of entities within the collection.
/// </summary>
public static class NinjadogEntitiesExtensions
{
    /// <summary>
    /// Converts the <see cref="NinjadogEntities"/> collection into a list of <see cref="NinjadogEntityWithKey"/>.
    /// This method facilitates the use of entities by associating each with its corresponding key.
    /// </summary>
    /// <param name="entities">The <see cref="NinjadogEntities"/> collection to be converted.</param>
    /// <returns>A list of <see cref="NinjadogEntityWithKey"/>.</returns>
    public static List<NinjadogEntityWithKey> FromKeys(
        this NinjadogEntities entities)
    {
        return
        [
            .. entities.Select(pair =>
            {
                var (key, value) = pair;
                return new NinjadogEntityWithKey(key, value.Properties, value.Relationships, value.SeedData);
            })
        ];
    }
}
