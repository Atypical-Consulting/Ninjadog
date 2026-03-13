using Ninjadog.Engine.Core.ValueObjects;

namespace Ninjadog.Engine.Infrastructure.Services;

/// <summary>
/// A service for creating a Ninjadog app.
/// </summary>
public class NinjadogAppService : INinjadogAppService
{
    private readonly NinjadogSettings _settings;
    private readonly NinjadogTemplateManifest _manifest;
    private readonly ICliDotnetService _dotnet;
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NinjadogAppService"/> class.
    /// </summary>
    /// <param name="settings">The Ninjadog settings.</param>
    /// <param name="manifest">The Ninjadog template manifest.</param>
    /// <param name="dotnet">The .NET CLI service.</param>
    /// <param name="fileService">The file service.</param>
    public NinjadogAppService(
        NinjadogSettings settings,
        NinjadogTemplateManifest manifest,
        ICliDotnetService dotnet,
        IFileService fileService)
    {
        _settings = settings;
        _manifest = manifest;
        _dotnet = dotnet;
        _fileService = fileService;

        AppName = settings.Config.Name;
        AppDirectory = Path.GetFullPath(settings.Config.OutputPath);
        Directory.CreateDirectory(AppDirectory);
        ProjectPath = Path.Combine(AppDirectory, "src", $"{AppName}.{_manifest.Name}");
    }

    /// <inheritdoc />
    public string AppName { get; }

    /// <inheritdoc />
    public string AppDirectory { get; }

    /// <inheritdoc />
    public string ProjectPath { get; }

    private string TestProjectPath
        => Path.Combine(AppDirectory, "src", $"{AppName}.IntegrationTests");

    /// <inheritdoc />
    public virtual async Task CreateAppAsync(bool deleteIfExists = true)
    {
        if (deleteIfExists && Directory.Exists(ProjectPath))
        {
            Directory.Delete(ProjectPath, true);
        }

        // Also clean the integration test project directory if it exists
        if (deleteIfExists && Directory.Exists(TestProjectPath))
        {
            Directory.Delete(TestProjectPath, true);
        }

        await NewNinjadogSettingsFileAsync();
        await NewEditorConfigFileAsync();
        await NewGitIgnoreFileAsync();
        await NewGlobalJsonFileAsync();
        await NewSolutionFileAsync();
        await NewProjectFileAsync();
        await AddProjectToSolutionAsync();
        await InstallNuGetPackages();
    }

    /// <inheritdoc />
    public async Task DotnetVersionAsync()
    {
        await _dotnet.ExecuteVersionAsync();
    }

    /// <inheritdoc />
    public virtual async Task NewNinjadogSettingsFileAsync()
    {
        var filePath = Path.Combine(AppDirectory, NinjadogSettingsFile);

        // If ninjadog.json already exists in the output directory (e.g. outputPath is "."),
        // preserve the original file to avoid data loss from re-serialization which can
        // strip fields like $schema, softDelete, auditing, etc.
        if (!File.Exists(filePath))
        {
            var jsonString = _settings.ToJsonString();
            _fileService.CreateFile(filePath, jsonString);
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task NewGitIgnoreFileAsync()
    {
        await _dotnet.ExecuteNewAsync("gitignore", AppDirectory);
    }

    /// <inheritdoc />
    public virtual async Task NewEditorConfigFileAsync()
    {
        await _dotnet.ExecuteNewAsync("editorconfig", AppDirectory);
    }

    /// <inheritdoc />
    public virtual async Task NewGlobalJsonFileAsync()
    {
        await _dotnet.ExecuteNewAsync("globaljson", AppDirectory);
    }

    /// <inheritdoc />
    public virtual async Task NewSolutionFileAsync()
    {
        await _dotnet.ExecuteNewSolutionAsync(AppDirectory);
    }

    /// <inheritdoc />
    public virtual async Task NewProjectFileAsync()
    {
        await _dotnet.ExecuteNewAsync("webapi", ProjectPath);
        SetRootNamespaceInProject();
    }

    /// <inheritdoc />
    public virtual async Task AddProjectToSolutionAsync()
    {
        await _dotnet.ExecuteAddProjectToSolutionAsync(AppDirectory, ProjectPath);
    }

    /// <inheritdoc />
    public virtual async Task AddFileToProjectAsync(NinjadogContentFile contentFile)
    {
        string path;

        if (contentFile.Category == IntegrationTestsCategory)
        {
            // Integration test files go in a sibling project directory, not inside the main project
            path = Path.Combine(TestProjectPath, contentFile.FileName);

            // When writing the test .csproj, also add it to the solution
            if (contentFile.FileName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                _fileService.CreateFile(path, contentFile.Content);
                await _dotnet.ExecuteAddProjectToSolutionAsync(AppDirectory, TestProjectPath);
                return;
            }
        }
        else
        {
            path = contentFile.Category is not null
                ? Path.Combine(ProjectPath, contentFile.Category, contentFile.FileName)
                : Path.Combine(ProjectPath, contentFile.FileName);
        }

        _fileService.CreateFile(path, contentFile.Content);
        await Task.CompletedTask;
    }

    private async Task InstallNuGetPackages()
    {
        foreach (var packageEntry in _manifest.NuGetPackages)
        {
            var parts = packageEntry.Split(':', 2);
            var packageName = parts[0];
            var version = parts.Length > 1 ? parts[1] : null;

            await _dotnet.ExecuteAddPackageAsync(ProjectPath, packageName, version);
        }
    }

    /// <summary>
    /// Sets the RootNamespace property in the generated .csproj file so that
    /// it matches the user-configured root namespace instead of defaulting to
    /// the project folder name (which includes the manifest suffix).
    /// </summary>
    private void SetRootNamespaceInProject()
    {
        var rootNamespace = _settings.Config.RootNamespace;
        var projectFolderName = $"{AppName}.{_manifest.Name}";

        // Skip if the root namespace already matches the project folder name
        if (rootNamespace == projectFolderName)
        {
            return;
        }

        var csprojPath = Path.Combine(ProjectPath, $"{projectFolderName}.csproj");
        if (!File.Exists(csprojPath))
        {
            return;
        }

        var content = File.ReadAllText(csprojPath);
        var rootNsElement = $"<RootNamespace>{rootNamespace}</RootNamespace>";

        // Insert RootNamespace after the first <PropertyGroup> tag
        content = content.Replace(
            "<PropertyGroup>",
            $"<PropertyGroup>\n    {rootNsElement}",
            StringComparison.Ordinal);

        File.WriteAllText(csprojPath, content);
    }
}
