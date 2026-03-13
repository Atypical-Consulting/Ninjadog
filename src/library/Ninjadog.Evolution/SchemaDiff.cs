namespace Ninjadog.Evolution;

/// <summary>
/// Represents the complete diff between two Ninjadog schema versions.
/// This is the top-level result returned by <see cref="SchemaDiffer.Compare"/>.
/// </summary>
/// <param name="EntityChanges">Changes to entities (added, removed, modified).</param>
/// <param name="ConfigChanges">Changes to the global configuration.</param>
/// <param name="EnumChanges">Changes to enum definitions.</param>
public sealed record SchemaDiff(
    IReadOnlyList<EntityDiff> EntityChanges,
    ConfigDiff ConfigChanges,
    IReadOnlyList<EnumDiff> EnumChanges)
{
    /// <summary>
    /// Gets a value indicating whether any schema changes were detected.
    /// </summary>
    public bool HasChanges =>
        EntityChanges.Count > 0
        || ConfigChanges.HasChanges
        || EnumChanges.Count > 0;

    /// <summary>
    /// Gets the entities that were added.
    /// </summary>
    public IEnumerable<EntityDiff> AddedEntities =>
        EntityChanges.Where(e => e.Kind == ChangeKind.Added);

    /// <summary>
    /// Gets the entities that were removed.
    /// </summary>
    public IEnumerable<EntityDiff> RemovedEntities =>
        EntityChanges.Where(e => e.Kind == ChangeKind.Removed);

    /// <summary>
    /// Gets the entities that were modified.
    /// </summary>
    public IEnumerable<EntityDiff> ModifiedEntities =>
        EntityChanges.Where(e => e.Kind == ChangeKind.Modified);

    /// <summary>
    /// Gets a SchemaDiff with no changes.
    /// </summary>
    public static SchemaDiff None { get; } = new([], ConfigDiff.None, []);
}
