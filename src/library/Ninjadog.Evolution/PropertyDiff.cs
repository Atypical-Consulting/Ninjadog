using Ninjadog.Settings.Entities.Properties;

namespace Ninjadog.Evolution;

/// <summary>
/// Represents a change to a single entity property between two schema versions.
/// </summary>
/// <param name="PropertyName">The name of the property that changed.</param>
/// <param name="Kind">Whether the property was added, removed, or modified.</param>
/// <param name="Before">The property definition before the change (null if added).</param>
/// <param name="After">The property definition after the change (null if removed).</param>
public sealed record PropertyDiff(
    string PropertyName,
    ChangeKind Kind,
    NinjadogEntityProperty? Before,
    NinjadogEntityProperty? After)
{
    /// <summary>
    /// Gets a value indicating whether the property type changed.
    /// </summary>
    public bool TypeChanged =>
        Kind == ChangeKind.Modified
        && Before is not null
        && After is not null
        && Before.Type != After.Type;

    /// <summary>
    /// Gets a value indicating whether only validation constraints changed (type remained the same).
    /// </summary>
    public bool OnlyConstraintsChanged =>
        Kind == ChangeKind.Modified
        && Before is not null
        && After is not null
        && Before.Type == After.Type;
}
