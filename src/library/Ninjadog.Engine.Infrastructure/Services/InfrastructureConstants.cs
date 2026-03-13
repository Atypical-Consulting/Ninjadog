namespace Ninjadog.Engine.Infrastructure.Services;

/// <summary>
/// Provides a set of constants used throughout the infrastructure layer of the Ninjadog Engine.
/// </summary>
public static class InfrastructureConstants
{
    /// <summary>
    /// Represents the special folder used to store user profiles.
    /// </summary>
    public const Environment.SpecialFolder UserProfile = Environment.SpecialFolder.UserProfile;

    /// <summary>
    /// The default directory name for Ninjadog projects within the user profile directory.
    /// </summary>
    public const string NinjadogProjects = "NinjadogProjects";

    /// <summary>
    /// The default file name for Ninjadog settings, stored in JSON format.
    /// </summary>
    public const string NinjadogSettingsFile = "ninjadog.json";

    /// <summary>
    /// The category name used by integration test templates.
    /// Files with this category are placed in a separate test project directory.
    /// </summary>
    public const string IntegrationTestsCategory = "IntegrationTests";

    /// <summary>
    /// Gets the base folder path for storing Ninjadog projects and configurations,
    /// typically located within the user's profile directory.
    /// </summary>
    public static string BaseFolder { get; } = Path.Combine(Environment.GetFolderPath(UserProfile), NinjadogProjects);
}
