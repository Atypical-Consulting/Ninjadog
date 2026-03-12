using Ninjadog.CLI.Utilities;

namespace Ninjadog.CLI.Commands;

internal sealed class BuildCommand(
    INinjadogEngineFactory engineFactory,
    IDomainEventDispatcher domainEventDispatcher,
    NinjadogVerbosityOptions verbosityOptions)
    : Command<BuildCommandSettings>
{
    public override int Execute(CommandContext context, BuildCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            verbosityOptions.Verbose = settings.Verbose;

            var displayService = new NinjadogEngineEventDisplayService(domainEventDispatcher, settings.Verbose);
            displayService.RegisterAllHandlers();

            var engine = engineFactory.CreateNinjadogEngine();

            if (settings.Verbose)
            {
                engine.Run();
            }
            else
            {
                RunWithLiveDisplay(engine, displayService);
            }

            return 0;
        }
        catch (Exception e)
        {
            WriteLine();
            WriteException(e);
            return 1;
        }
    }

    private static void RunWithLiveDisplay(INinjadogEngine engine, NinjadogEngineEventDisplayService displayService)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Status[/]").Width(12))
            .AddColumn(new TableColumn("[bold]File[/]"))
            .AddColumn(new TableColumn("[bold]Size[/]").RightAligned().Width(10));

        AnsiConsole.Live(table)
            .AutoClear(true)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx =>
            {
                displayService.SetLiveContext(ctx, table);
                engine.Run();
            });

        var snapshot = engine.Context.GetSnapshot();
        var elapsed = snapshot.TotalTimeElapsed;
        var totalFiles = snapshot.TotalFilesGenerated;
        var totalChars = snapshot.TotalCharactersGenerated;

        WriteLine();
        MarkupLine($"[bold green]Build completed[/] in [bold]{elapsed.TotalSeconds:F1}s[/] — [green]{totalFiles} files[/] generated ({totalChars:N0} chars)");
        WriteLine();
    }
}
