namespace Ninjadog.Settings.Config;

/// <summary>
/// Represents the base configuration for a Ninjadog Engine application.
/// This abstract record defines common configuration properties that are essential
/// for initializing and running the Ninjadog Engine.
/// </summary>
/// <param name="Name">The name of the application or project.</param>
/// <param name="Version">The version of the application or project.</param>
/// <param name="Description">A brief description of the application or project.</param>
/// <param name="RootNamespace">The root namespace for the generated code.</param>
/// <param name="OutputPath">The path where the generated files will be saved.</param>
/// <param name="SaveGeneratedFiles">Indicates whether the generated files should be saved to disk. Default is true.</param>
/// <param name="Cors">The optional CORS configuration for the application.</param>
/// <param name="SoftDelete">Indicates whether soft delete is enabled. When true, DELETE operations set IsDeleted=1 instead of removing rows. Default is false.</param>
/// <param name="Auditing">Indicates whether audit fields (CreatedAt, UpdatedAt) should be added to generated tables. Default is false.</param>
/// <param name="DatabaseProvider">The database provider to use (sqlite, postgresql, sqlserver). Default is sqlite.</param>
/// <param name="Aot">Indicates whether Native AOT publishing support should be enabled in the generated project. Default is false.</param>
/// <param name="Auth">The optional JWT authentication configuration for the application.</param>
public abstract record NinjadogConfiguration(
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
    NinjadogAuthConfiguration? Auth = null);
