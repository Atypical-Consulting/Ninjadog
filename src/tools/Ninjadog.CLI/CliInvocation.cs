namespace Ninjadog.CLI;

/// <summary>
/// Classifies CLI invocations based on whether they require project settings during startup.
/// </summary>
public static class CliInvocation
{
    private static readonly HashSet<string> _commandsRequiringProjectSettings = new(StringComparer.OrdinalIgnoreCase)
    {
        "build",
    };

    /// <summary>
    /// Determines whether the current CLI invocation needs <c>ninjadog.json</c> to be loaded during startup.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    /// <returns><see langword="true"/> when project settings should be loaded before command execution; otherwise, <see langword="false"/>.</returns>
    public static bool RequiresProjectSettings(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0 || args.Any(IsHelpOrVersionSwitch))
        {
            return false;
        }

        var commandName = args.FirstOrDefault(static arg =>
            !string.IsNullOrWhiteSpace(arg) &&
            arg[0] != '-');

        return commandName is not null && _commandsRequiringProjectSettings.Contains(commandName);
    }

    private static bool IsHelpOrVersionSwitch(string arg)
    {
        return arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
               arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
               arg.Equals("-?", StringComparison.OrdinalIgnoreCase) ||
               arg.Equals("--version", StringComparison.OrdinalIgnoreCase);
    }
}
