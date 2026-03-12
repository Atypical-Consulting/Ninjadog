namespace Ninjadog.Engine.Infrastructure.Services;

/// <summary>
/// A service for handling file and directory operations.
/// </summary>
public class FileService : IFileService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    public FileService()
    {
        EnsureDirectoryExists(BaseFolder);
    }

    /// <inheritdoc />
    public string CreateAppFolder(string appName)
    {
        var appFolderPath = Path.Combine(BaseFolder, appName);
        EnsureDirectoryExists(appFolderPath);
        return appFolderPath;
    }

    /// <inheritdoc />
    public void DeleteAppFolder(string appName)
    {
        var appFolderPath = Path.Combine(BaseFolder, appName);
        EnsureDirectoryExists(appFolderPath);
        Directory.Delete(appFolderPath, true);
    }

    /// <inheritdoc />
    public string CreateSubFolder(string appName, string subFolderName)
    {
        var subFolderPath = Path.Combine(BaseFolder, appName, subFolderName);
        EnsureDirectoryExists(subFolderPath);
        return subFolderPath;
    }

    /// <inheritdoc />
    public string CreateFile(string path, string content)
    {
        var filePath = Path.Combine(BaseFolder, path);
        EnsureDirectoryExists(Path.GetDirectoryName(filePath) ?? string.Empty);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    /// <inheritdoc />
    [Obsolete("Use NinjadogAppService instead.")]
    public string CreateNinjadogSettingsFile(string appName, NinjadogSettings ninjadogSettings)
    {
        var filePath = Path.Combine(BaseFolder, appName, NinjadogSettingsFile);
        var jsonString = ninjadogSettings.ToJsonString();
        File.WriteAllText(filePath, jsonString);
        return filePath;
    }

    /// <summary>
    /// Ensures that a directory exists, creating it if it does not.
    /// </summary>
    /// <param name="path">The path of the directory to check or create.</param>
    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
