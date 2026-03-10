// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Engine.Core.Abstractions;

/// <summary>
/// Defines the interface for the Ninjadog application service,
/// responsible for initializing and setting up various components of a Ninjadog application.
/// </summary>
public interface INinjadogAppService
{
    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    string AppName { get; }

    /// <summary>
    /// Gets the directory of the application.
    /// </summary>
    string AppDirectory { get; }

    /// <summary>
    /// Gets the path of the project file for the application.
    /// </summary>
    string ProjectPath { get; }

    /// <summary>
    /// Creates the application based on the initialized settings and manifest.
    /// </summary>
    /// <param name="deleteIfExists">Whether to delete the application folder if it already exists.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateAppAsync(bool deleteIfExists = true);

    /// <summary>
    /// Executes the dotnet CLI version command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DotnetVersionAsync();

    /// <summary>
    /// Creates a Ninjadog settings file for the application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NewNinjadogSettingsFileAsync();

    /// <summary>
    /// Creates a .gitignore file suitable for the application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NewGitIgnoreFileAsync();

    /// <summary>
    /// Creates an EditorConfig file to maintain consistent coding styles for various editors and IDEs.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NewEditorConfigFileAsync();

    /// <summary>
    /// Creates a global.json file to specify the .NET SDK version to be used.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NewGlobalJsonFileAsync();

    /// <summary>
    /// Creates the solution file for the application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NewSolutionFileAsync();

    /// <summary>
    /// Creates the project file for the application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NewProjectFileAsync();

    /// <summary>
    /// Adds the generated project to the solution file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task AddProjectToSolutionAsync();

    /// <summary>
    /// Creates a new file in the project.
    /// </summary>
    /// <param name="contentFile">The content file to be created.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task AddFileToProjectAsync(NinjadogContentFile contentFile);
}
