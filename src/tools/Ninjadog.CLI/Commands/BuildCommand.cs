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
                RunWithProgress(engine, displayService);
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

    private static void RunWithProgress(INinjadogEngine engine, NinjadogEngineEventDisplayService displayService)
    {
        AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .Start(ctx =>
            {
                displayService.SetProgressContext(ctx);
                engine.Run();
            });

        var snapshot = engine.Context.GetSnapshot();
        var elapsed = snapshot.TotalTimeElapsed;
        var totalFiles = snapshot.TotalFilesGenerated;

        WriteLine();
        MarkupLine($"[bold]Build completed in {elapsed.TotalSeconds:F1}s[/] — [green]{totalFiles} files[/] generated");
        WriteLine();
    }
}
