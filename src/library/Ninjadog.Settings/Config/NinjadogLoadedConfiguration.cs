namespace Ninjadog.Settings.Config;

/// <summary>
/// A concrete implementation of <see cref="NinjadogConfiguration"/> used when loading configuration from a JSON file.
/// </summary>
public sealed record NinjadogLoadedConfiguration(
    string Name,
    string Version,
    string Description,
    string RootNamespace,
    string OutputPath,
    bool SaveGeneratedFiles = true,
    NinjadogCorsConfiguration? Cors = null,
    bool SoftDelete = false,
    bool Auditing = false,
    string DatabaseProvider = "sqlite",
    bool Aot = false,
    NinjadogAuthConfiguration? Auth = null)
    : NinjadogConfiguration(Name, Version, Description, RootNamespace, OutputPath, SaveGeneratedFiles, Cors, SoftDelete, Auditing, DatabaseProvider, Aot, Auth);
