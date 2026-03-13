namespace Ninjadog.Evolution;

/// <summary>
/// Represents changes to the global configuration between two schema versions.
/// </summary>
/// <param name="SoftDeleteChanged">Whether the soft delete setting changed.</param>
/// <param name="SoftDeleteEnabled">The new soft delete value (null if unchanged).</param>
/// <param name="AuditingChanged">Whether the auditing setting changed.</param>
/// <param name="AuditingEnabled">The new auditing value (null if unchanged).</param>
/// <param name="DatabaseProviderChanged">Whether the database provider changed.</param>
/// <param name="OldDatabaseProvider">The previous database provider (null if unchanged).</param>
/// <param name="NewDatabaseProvider">The new database provider (null if unchanged).</param>
/// <param name="AotChanged">Whether the AOT setting changed.</param>
/// <param name="CorsChanged">Whether the CORS configuration changed.</param>
/// <param name="AuthChanged">Whether the auth configuration changed.</param>
/// <param name="RateLimitChanged">Whether the rate limit configuration changed.</param>
/// <param name="VersioningChanged">Whether the versioning configuration changed.</param>
public sealed record ConfigDiff(
    bool SoftDeleteChanged,
    bool? SoftDeleteEnabled,
    bool AuditingChanged,
    bool? AuditingEnabled,
    bool DatabaseProviderChanged,
    string? OldDatabaseProvider,
    string? NewDatabaseProvider,
    bool AotChanged,
    bool CorsChanged,
    bool AuthChanged,
    bool RateLimitChanged,
    bool VersioningChanged)
{
    /// <summary>
    /// Gets a value indicating whether any configuration changed.
    /// </summary>
    public bool HasChanges =>
        SoftDeleteChanged
        || AuditingChanged
        || DatabaseProviderChanged
        || AotChanged
        || CorsChanged
        || AuthChanged
        || RateLimitChanged
        || VersioningChanged;

    /// <summary>
    /// Gets a ConfigDiff with no changes.
    /// </summary>
    public static ConfigDiff None { get; } = new(
        false,
        null,
        false,
        null,
        false,
        null,
        null,
        false,
        false,
        false,
        false,
        false);
}
