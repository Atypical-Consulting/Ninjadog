using System.Runtime.CompilerServices;

namespace Ninjadog.Tests;

public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.ScrubLinesWithReplace(line =>
            line.Contains("Generated on:")
                ? "// Generated on: [SCRUBBED]"
                : line.Contains("// Version")
                    ? "// Version     : [SCRUBBED]"
                    : line);
    }
}
