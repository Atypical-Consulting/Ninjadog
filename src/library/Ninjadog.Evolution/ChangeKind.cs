namespace Ninjadog.Evolution;

/// <summary>
/// Represents the type of change detected between two schema versions.
/// </summary>
public enum ChangeKind
{
    /// <summary>
    /// An element was added in the new schema.
    /// </summary>
    Added,

    /// <summary>
    /// An element was removed from the new schema.
    /// </summary>
    Removed,

    /// <summary>
    /// An element was modified between schema versions.
    /// </summary>
    Modified
}
