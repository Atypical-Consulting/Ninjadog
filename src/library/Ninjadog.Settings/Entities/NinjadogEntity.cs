using Ninjadog.Settings.Entities.Properties;

namespace Ninjadog.Settings.Entities;

/// <summary>
/// Represents a Ninjadog entity definition.
/// This record holds the properties of an entity, which can include fields, data types, and key information.
/// It is used to define the structure and characteristics of an entity within the Ninjadog framework.
/// </summary>
/// <param name="Properties">The properties of the entity, including their types and key designation.</param>
/// <param name="Relationships">The relationships between this entity and other entities.</param>
public record NinjadogEntity(
    NinjadogEntityProperties Properties,
    NinjadogEntityRelationships? Relationships = null,
    List<Dictionary<string, object>>? SeedData = null);
