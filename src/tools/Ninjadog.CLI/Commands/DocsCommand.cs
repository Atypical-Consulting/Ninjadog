using Ninjadog.CLI.Utilities;

namespace Ninjadog.CLI.Commands;

/// <summary>
/// Opens the Ninjadog documentation website in the default browser.
/// </summary>
internal sealed class DocsCommand : Command<DocsCommandSettings>
{
    private const string DocsUrl = "https://atypical-consulting.github.io/Ninjadog/";

    public override int Execute(CommandContext context, DocsCommandSettings settings, CancellationToken cancellationToken)
    {
        MarkupLine($"[green]Opening documentation:[/] [blue]{DocsUrl}[/]");

        try
        {
            BrowserHelper.OpenBrowser(DocsUrl);
        }
        catch
        {
            MarkupLine($"[yellow]Could not open browser automatically. Please visit:[/] [blue]{DocsUrl}[/]");
        }

        return 0;
    }
}
