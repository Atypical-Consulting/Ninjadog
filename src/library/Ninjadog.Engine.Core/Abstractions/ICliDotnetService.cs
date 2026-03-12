namespace Ninjadog.Engine.Core.Abstractions;

/// <summary>
/// Defines the interface for a service capable of executing dotnet CLI commands.
/// This interface abstracts the functionality required to run various dotnet commands programmatically.
/// </summary>
public interface ICliDotnetService
{
    /// <summary>
    /// Executes the dotnet CLI version command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteVersionAsync();

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="templateKey">The template of the project to create (e.g., 'console', 'webapi').</param>
    /// <param name="outputPath">The output directory for the created project.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteNewAsync(string templateKey, string outputPath);

    /// <summary>
    /// Executes the dotnet CLI new command (with the 'sln' template).
    /// </summary>
    /// <param name="solutionPath">The path to the solution to be created.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteNewSolutionAsync(string solutionPath);

    /// <summary>
    /// Builds a project at the specified path.
    /// </summary>
    /// <param name="projectPath">The path to the project to be built.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteBuildAsync(string projectPath);

    /// <summary>
    /// Adds a NuGet package to a project.
    /// </summary>
    /// <param name="projectPath">The path to the project to which the package will be added.</param>
    /// <param name="package">The name of the package to be added.</param>
    /// <param name="version">The optional version of the package to be added. When null, the latest version is installed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteAddPackageAsync(string projectPath, string package, string? version = null);

    /// <summary>
    /// Adds a project to a solution file.
    /// </summary>
    /// <param name="solutionDirectory">The directory containing the solution file.</param>
    /// <param name="projectPath">The path to the project to add to the solution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteAddProjectToSolutionAsync(string solutionDirectory, string projectPath);

    /// <summary>
    /// Publishes a project with Ahead-of-Time (AOT) compilation.
    /// </summary>
    /// <param name="projectPath">The path to the project to publish.</param>
    /// <param name="runtimeIdentifier">The runtime identifier (e.g., win-x64, linux-x64).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecutePublishWithAOTAsync(string projectPath, string runtimeIdentifier);
}
