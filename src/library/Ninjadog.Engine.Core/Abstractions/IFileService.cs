namespace Ninjadog.Engine.Core.Abstractions;

/// <summary>
/// A service for handling file and directory operations.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Creates a subdirectory within the NinjadogProject directory.
    /// </summary>
    /// <param name="appName">The name of the application, used as the subdirectory name.</param>
    /// <returns>The path to the newly created directory.</returns>
    string CreateAppFolder(string appName);

    /// <summary>
    /// Deletes an application's directory.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    void DeleteAppFolder(string appName);

    /// <summary>
    /// Creates a subdirectory within an application's directory.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="subFolderName">The name of the subfolder to create.</param>
    /// <returns>The path to the newly created directory.</returns>
    string CreateSubFolder(string appName, string subFolderName);

    /// <summary>
    /// Creates a file with content in a specified directory.
    /// </summary>
    /// <param name="path">The path to the directory where the file will be created.</param>
    /// <param name="content">The content to write to the file.</param>
    /// <returns>The path to the newly created file.</returns>
    string CreateFile(string path, string content);

    /// <summary>
    /// Creates a Ninjadog settings file in the specified directory.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="ninjadogSettings">The Ninjadog settings to write to the file.</param>
    /// <returns>The path to the newly created file.</returns>
    /// <remarks>
    /// The Ninjadog settings file is a JSON file containing the settings used by the Ninjadog Engine
    /// to generate the application's content.
    /// </remarks>
    string CreateNinjadogSettingsFile(string appName, NinjadogSettings ninjadogSettings);
}
