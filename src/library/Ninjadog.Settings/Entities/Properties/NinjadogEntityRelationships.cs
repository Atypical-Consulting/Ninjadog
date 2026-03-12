namespace Ninjadog.Settings.Entities.Properties;

/// <summary>
/// Represents a collection of relationships for a Ninjadog entity.
/// This class extends a dictionary to map entity names to their corresponding Ninjadog entity relationships.
/// It's used to define the relationships between entities.
/// </summary>
public class NinjadogEntityRelationships
    : Dictionary<string, NinjadogEntityRelationship>;
