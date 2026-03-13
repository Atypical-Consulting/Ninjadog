using System.Globalization;
using System.Text;

namespace Ninjadog.Evolution.Migrations;

/// <summary>
/// Writes migration operations to numbered SQL files in the .ninjadog/migrations/ directory.
/// </summary>
public static class MigrationFileWriter
{
    /// <summary>
    /// The directory name for migration files within the .ninjadog state directory.
    /// </summary>
    public const string MigrationsDirectoryName = "migrations";

    /// <summary>
    /// Writes a set of migration operations to a single SQL migration file.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    /// <param name="operations">The migration operations to write.</param>
    /// <param name="migrationName">An optional name for the migration file.</param>
    /// <returns>The path to the generated migration file, or null if no operations.</returns>
    public static string? Write(
        string projectRoot,
        IReadOnlyList<MigrationOperation> operations,
        string? migrationName = null)
    {
        if (operations.Count == 0)
        {
            return null;
        }

        var migrationsDir = Path.Combine(projectRoot, SchemaState.StateDirectoryName, MigrationsDirectoryName);
        Directory.CreateDirectory(migrationsDir);

        var nextNumber = GetNextMigrationNumber(migrationsDir);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var safeName = SanitizeName(migrationName ?? "evolution");
        var fileName = $"{nextNumber:D3}_{timestamp}_{safeName}.sql";
        var filePath = Path.Combine(migrationsDir, fileName);

        var content = BuildMigrationContent(operations);
        File.WriteAllText(filePath, content);

        return filePath;
    }

    /// <summary>
    /// Gets the next migration number based on existing files in the migrations directory.
    /// </summary>
    /// <param name="migrationsDir">The migrations directory path.</param>
    /// <returns>The next sequential migration number.</returns>
    internal static int GetNextMigrationNumber(string migrationsDir)
    {
        if (!Directory.Exists(migrationsDir))
        {
            return 1;
        }

        var existingFiles = Directory.GetFiles(migrationsDir, "*.sql");

        if (existingFiles.Length == 0)
        {
            return 1;
        }

        var maxNumber = 0;
        foreach (var file in existingFiles)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var underscoreIndex = name.IndexOf('_');
            if (underscoreIndex > 0 && int.TryParse(name[..underscoreIndex], out var number))
            {
                maxNumber = Math.Max(maxNumber, number);
            }
        }

        return maxNumber + 1;
    }

    private static string BuildMigrationContent(IReadOnlyList<MigrationOperation> operations)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var sb = new StringBuilder()
            .AppendLine("-- Ninjadog Schema Evolution Migration")
            .AppendLine(CultureInfo.InvariantCulture, $"-- Generated at {timestamp} UTC")
            .AppendLine();

        for (var i = 0; i < operations.Count; i++)
        {
            var op = operations[i];
            var header = op.IsWarning
                ? $"-- WARNING: {op.Description}"
                : $"-- {op.Description}";

            sb.AppendLine(CultureInfo.InvariantCulture, $"{header}")
                .AppendLine(op.Sql);

            if (i < operations.Count - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string SanitizeName(string name)
    {
        var sanitized = new StringBuilder();
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                sanitized.Append(char.ToLowerInvariant(c));
            }
            else if (c is ' ' or '-' or '_')
            {
                sanitized.Append('_');
            }
        }

        return sanitized.ToString();
    }
}
