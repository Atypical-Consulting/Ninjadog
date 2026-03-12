namespace Ninjadog.Templates.CrudWebAPI.UseCases.TodoApp;

/// <summary>
/// Represents the settings for the TodoApp use case.
/// </summary>
public record TodoAppSettings()
    : NinjadogSettings(
        new TodoAppConfiguration(),
        new TodoAppEntities());
