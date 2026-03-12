namespace Ninjadog.Settings.Entities.Properties;

/// <summary>
/// Represents a relationship between two Ninjadog entities.
/// This record is used to define the type of relationship (e.g., one-to-many, many-to-one) between entities.
/// </summary>
/// <param name="RelatedEntity">The name of the entity that this relationship is associated with.</param>
/// <param name="RelationshipType">The type of relationship.</param>
public record NinjadogEntityRelationship(
    string RelatedEntity,
    NinjadogEntityRelationshipType RelationshipType);
