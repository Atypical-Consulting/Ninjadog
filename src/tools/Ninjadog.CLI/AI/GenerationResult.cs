using Ninjadog.Settings.Validation;

namespace Ninjadog.CLI.AI;

internal sealed record GenerationResult(
    bool Success,
    string? Json,
    string? AssistantMessage,
    SchemaValidationResult? Validation,
    string? Error)
{
    internal static GenerationResult Failure(string error)
    {
        return new(false, null, null, null, error);
    }
}
