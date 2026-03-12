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
            engineFactory.CreateNinjadogEngine().Run();
            return 0;
        }
        catch (Exception e)
        {
            WriteLine();
            WriteException(e);
            return 1;
        }
    }
}
