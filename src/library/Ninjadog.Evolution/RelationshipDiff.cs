using Ninjadog.Settings.Entities.Properties;

namespace Ninjadog.Evolution;

/// <summary>
/// Represents a change to a single entity relationship between two schema versions.
/// </summary>
/// <param name="RelationshipName">The name of the relationship that changed.</param>
/// <param name="Kind">Whether the relationship was added, removed, or modified.</param>
/// <param name="Before">The relationship definition before the change (null if added).</param>
/// <param name="After">The relationship definition after the change (null if removed).</param>
public sealed record RelationshipDiff(
    string RelationshipName,
    ChangeKind Kind,
    NinjadogEntityRelationship? Before,
    NinjadogEntityRelationship? After);
