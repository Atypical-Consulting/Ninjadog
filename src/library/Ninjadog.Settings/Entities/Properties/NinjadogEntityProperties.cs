namespace Ninjadog.Settings.Entities.Properties;

/// <summary>
/// Represents a collection of properties for a Ninjadog entity.
/// This class extends a dictionary to map property names to their corresponding Ninjadog entity properties.
/// It's used to define the attributes and characteristics of each property in an entity.
/// </summary>
public class NinjadogEntityProperties
    : Dictionary<string, NinjadogEntityProperty>;
