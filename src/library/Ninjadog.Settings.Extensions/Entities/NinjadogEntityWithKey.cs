namespace Ninjadog.Settings.Extensions.Entities;

/// <summary>
/// Represents a Ninjadog entity along with its key.
/// This record extends <see cref="NinjadogEntity"/> by associating it with a specific key,
/// enabling more specific template generation based on the key-value relationship.
/// </summary>
/// <param name="Key">The key of the entity.</param>
/// <param name="Properties">The properties of the entity.</param>
/// <param name="Relationships">The relationships between this entity and other entities.</param>
/// <param name="SeedData">The seed data rows to insert when initializing the database.</param>
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
    /// <param name="excludeAutoKey">When true, excludes key properties with auto-generated types (e.g., Guid).</param>
    /// <returns>A <see cref="string"/> containing the C# code for all member properties of the entity.</returns>
    public string GenerateMemberProperties(bool excludeAutoKey = false)
    {
        var props = Properties.FromKeys();

        if (excludeAutoKey)
        {
            props = props.Where(p => !(p.IsKey && p.Type == "Guid")).ToList();
        }

        var properties = props
            .Select(p => p.GenerateMemberProperty())
            .Aggregate((x, y) => $"{x}\n{y}");

        return properties;
    }
}
