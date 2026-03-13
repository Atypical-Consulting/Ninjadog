namespace Ninjadog.Evolution.Migrations;

/// <summary>
/// Represents a single migration operation with its SQL and a human-readable description.
/// </summary>
/// <param name="Description">A human-readable description of what this operation does.</param>
/// <param name="Sql">The SQL statement(s) to execute.</param>
/// <param name="IsWarning">Indicates the operation requires manual review or may not be fully automatable.</param>
public sealed record MigrationOperation(
    string Description,
    string Sql,
    bool IsWarning = false);
