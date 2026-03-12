namespace Ninjadog.Settings.Extensions.Entities.Properties;

/// <summary>
/// Provides extension methods for <see cref="NinjadogEntityProperties"/> collections.
/// These methods enhance the functionality of <see cref="NinjadogEntityProperties"/>, offering additional utilities
/// such as conversion and manipulation of entity properties within the collection.
/// </summary>
public static class NinjadogEntityPropertiesExtensions
{
    /// <summary>
    /// Converts the <see cref="NinjadogEntityProperties"/> collection into a list of <see cref="NinjadogEntityPropertyWithKey"/>.
    /// This method facilitates the use of entity properties by associating each with its corresponding key,
    /// thus providing a convenient way to handle entity properties with their identifying keys.
    /// </summary>
    /// <param name="properties">The <see cref="NinjadogEntityProperties"/> collection to be converted.</param>
    /// <returns>A list of <see cref="NinjadogEntityPropertyWithKey"/>, each representing a property along with its key.</returns>
    public static List<NinjadogEntityPropertyWithKey> FromKeys(
        this NinjadogEntityProperties properties)
    {
        return
        [
            .. properties.Select(x => new NinjadogEntityPropertyWithKey(x.Key, x.Value))
        ];
    }

    /// <summary>
    /// Gets the entity key from the <see cref="NinjadogEntityProperties"/> collection.
    /// </summary>
    /// <param name="properties">The <see cref="NinjadogEntityProperties"/> collection to be searched.</param>
    /// <returns>The <see cref="NinjadogEntityId"/> representing the entity key.</returns>
    public static NinjadogEntityPropertyWithKey GetEntityKey(
        this NinjadogEntityProperties properties)
    {
        var entityId = properties
            .FirstOrDefault(x => x.Value.IsKey);

        return new NinjadogEntityPropertyWithKey(entityId.Key, entityId.Value);
    }
}
