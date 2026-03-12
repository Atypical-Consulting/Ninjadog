namespace Ninjadog.Settings.Extensions.Entities.Properties;

/// <summary>
/// Represents a special entity property that is designated as an identifier (ID).
/// This record extends <see cref="NinjadogEntityProperty"/>, marking the property as a key by default
/// and setting its type to <see cref="Guid"/>, commonly used for unique identifiers.
/// </summary>
public record NinjadogEntityId()
    : NinjadogEntityProperty<Guid>(true);
