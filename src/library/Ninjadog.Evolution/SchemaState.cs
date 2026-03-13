namespace Ninjadog.Evolution;

/// <summary>
/// Manages the schema state file (.ninjadog/state.json) that tracks the last known
/// ninjadog.json configuration. This state is used to compute diffs between schema versions.
/// </summary>
public static class SchemaState
{
    /// <summary>
    /// The directory name where Ninjadog state files are stored.
    /// </summary>
    public const string StateDirectoryName = ".ninjadog";

    /// <summary>
    /// The file name for the schema state snapshot.
    /// </summary>
    public const string StateFileName = "state.json";

    /// <summary>
    /// Saves a copy of the current ninjadog.json content as the schema state.
    /// Creates the .ninjadog directory if it does not exist.
    /// </summary>
    /// <param name="projectRoot">The project root directory containing ninjadog.json.</param>
    /// <param name="ninjadogJsonContent">The raw JSON content of ninjadog.json to snapshot.</param>
    public static void Save(string projectRoot, string ninjadogJsonContent)
    {
        var stateDir = Path.Combine(projectRoot, StateDirectoryName);
        Directory.CreateDirectory(stateDir);

        var statePath = Path.Combine(stateDir, StateFileName);
        File.WriteAllText(statePath, ninjadogJsonContent);
    }

    /// <summary>
    /// Loads the previously saved schema state JSON content.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    /// <returns>The raw JSON content, or null if no state file exists.</returns>
    public static string? Load(string projectRoot)
    {
        var statePath = Path.Combine(projectRoot, StateDirectoryName, StateFileName);

        if (!File.Exists(statePath))
        {
            return null;
        }

        return File.ReadAllText(statePath);
    }

    /// <summary>
    /// Checks whether a schema state file exists.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    /// <returns>True if state.json exists in .ninjadog/.</returns>
    public static bool Exists(string projectRoot)
    {
        var statePath = Path.Combine(projectRoot, StateDirectoryName, StateFileName);
        return File.Exists(statePath);
    }
}
