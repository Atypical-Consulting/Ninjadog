namespace Ninjadog.Evolution;

/// <summary>
/// Represents all changes to a single entity between two schema versions.
/// </summary>
/// <param name="EntityKey">The entity key (e.g. "TodoItem").</param>
/// <param name="Kind">Whether the entity was added, removed, or modified.</param>
/// <param name="PropertyChanges">Changes to the entity's properties.</param>
/// <param name="RelationshipChanges">Changes to the entity's relationships.</param>
public sealed record EntityDiff(
    string EntityKey,
    ChangeKind Kind,
    IReadOnlyList<PropertyDiff> PropertyChanges,
    IReadOnlyList<RelationshipDiff> RelationshipChanges)
{
    /// <summary>
    /// Gets a value indicating whether any property types changed (requires migration attention).
    /// </summary>
    public bool HasTypeChanges =>
        PropertyChanges.Any(p => p.TypeChanged);

    /// <summary>
    /// Gets the properties that were added.
    /// </summary>
    public IEnumerable<PropertyDiff> AddedProperties =>
        PropertyChanges.Where(p => p.Kind == ChangeKind.Added);

    /// <summary>
    /// Gets the properties that were removed.
    /// </summary>
    public IEnumerable<PropertyDiff> RemovedProperties =>
        PropertyChanges.Where(p => p.Kind == ChangeKind.Removed);

    /// <summary>
    /// Gets the properties that were modified.
    /// </summary>
    public IEnumerable<PropertyDiff> ModifiedProperties =>
        PropertyChanges.Where(p => p.Kind == ChangeKind.Modified);
}
