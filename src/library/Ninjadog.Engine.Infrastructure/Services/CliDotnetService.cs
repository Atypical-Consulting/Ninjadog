using CliWrap;
using CliWrap.EventStream;
using Spectre.Console;

namespace Ninjadog.Engine.Infrastructure.Services;

/// <summary>
/// A service for executing .NET CLI commands.
/// This class provides functionality to programmatically run commands using the .NET CLI,
/// ensuring the CLI is available upon initialization.
/// </summary>
public sealed class CliDotnetService(NinjadogVerbosityOptions verbosityOptions) : ICliDotnetService
{
    private const string DotnetCommand = "dotnet";

    /// <inheritdoc />
    public async Task ExecuteVersionAsync()
    {
        var cmd = Cli
            .Wrap(DotnetCommand)
            .WithArguments("--version");

        await ListenCommandAsync(cmd);
    }

    /// <inheritdoc />
    public async Task ExecuteNewAsync(string templateKey, string outputPath)
    {
        var cmd = Cli
            .Wrap(DotnetCommand)
            .WithArguments(["new", templateKey, "--output", outputPath]);

        await ListenCommandAsync(cmd);
    }

    /// <inheritdoc />
    public async Task ExecuteNewSolutionAsync(string solutionPath)
    {
        await ExecuteNewAsync("sln", solutionPath);
    }

    /// <inheritdoc />
    public async Task ExecuteBuildAsync(string projectPath)
    {
        var cmd = Cli
            .Wrap(DotnetCommand)
            .WithArguments(["build", projectPath]);

        await ListenCommandAsync(cmd);
    }

    /// <inheritdoc />
    public async Task ExecuteAddPackageAsync(string projectPath, string package, string? version = null)
    {
        var arguments = new List<string> { "add", projectPath, "package", package };

        if (!string.IsNullOrWhiteSpace(version))
        {
            arguments.Add("--version");
            arguments.Add(version);
        }

        var cmd = Cli
            .Wrap(DotnetCommand)
            .WithArguments(arguments);

        await ListenCommandAsync(cmd);
    }

    /// <inheritdoc />
    public async Task ExecuteAddProjectToSolutionAsync(string solutionDirectory, string projectPath)
    {
        var cmd = Cli
            .Wrap(DotnetCommand)
            .WithArguments(["sln", solutionDirectory, "add", projectPath]);

        await ListenCommandAsync(cmd);
    }

    /// <inheritdoc />
    public async Task ExecutePublishWithAOTAsync(string projectPath, string runtimeIdentifier)
    {
        var cmd = Cli
            .Wrap(DotnetCommand)
            .WithArguments(
            [
                "publish",
                projectPath,
                "-c",
                "Release",
                "-r",
                runtimeIdentifier,
                "--self-contained",
                "-p:PublishReadyToRun=true"
            ]);

        await ListenCommandAsync(cmd);
    }

    private async Task ListenCommandAsync(Command cmd)
    {
        var verbose = verbosityOptions.Verbose;

        await foreach (var cmdEvent in cmd.ListenAsync())
        {
            switch (cmdEvent)
            {
                case StartedCommandEvent started when verbose:
                    AnsiConsole.MarkupLine($"[yellow]Process started; ID: {started.ProcessId}[/]");
                    break;
                case StandardOutputCommandEvent stdOut when verbose:
                    AnsiConsole.MarkupLine($"[blue]Out>[/] {stdOut.Text}");
                    break;
                case StandardErrorCommandEvent stdErr:
                    AnsiConsole.MarkupLine($"[red]Err>[/] {stdErr.Text}");
                    break;
                case ExitedCommandEvent { ExitCode: not 0 } exited:
                    AnsiConsole.MarkupLine($"[red]Process exited with code {exited.ExitCode}[/]");
                    AnsiConsole.WriteLine();
                    break;
                case ExitedCommandEvent when verbose:
                    AnsiConsole.MarkupLine("[green]Process completed successfully[/]");
                    AnsiConsole.WriteLine();
                    break;
            }
        }
    }
}
