namespace Ninjadog.Evolution;

/// <summary>
/// Represents a change to an enum definition between two schema versions.
/// </summary>
/// <param name="EnumName">The name of the enum that changed.</param>
/// <param name="Kind">Whether the enum was added, removed, or modified.</param>
/// <param name="Before">The enum values before the change (null if added).</param>
/// <param name="After">The enum values after the change (null if removed).</param>
public sealed record EnumDiff(
    string EnumName,
    ChangeKind Kind,
    IReadOnlyList<string>? Before,
    IReadOnlyList<string>? After)
{
    /// <summary>
    /// Gets the values that were added to the enum.
    /// </summary>
    public IReadOnlyList<string> AddedValues =>
        Kind == ChangeKind.Modified && Before is not null && After is not null
            ? [.. After.Except(Before)]
            : Kind == ChangeKind.Added && After is not null
                ? After
                : [];

    /// <summary>
    /// Gets the values that were removed from the enum.
    /// </summary>
    public IReadOnlyList<string> RemovedValues =>
        Kind == ChangeKind.Modified && Before is not null && After is not null
            ? [.. Before.Except(After)]
            : Kind == ChangeKind.Removed && Before is not null
                ? Before
                : [];
}
