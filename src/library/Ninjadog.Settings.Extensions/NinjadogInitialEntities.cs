namespace Ninjadog.Settings.Extensions;

/// <summary>
/// Defines the entities for the Ninjadog use case.
/// </summary>
public sealed class NinjadogInitialEntities : NinjadogEntities
{
    private const string Person = "Person";

    /// <summary>
    /// Initializes a new instance of the <see cref="NinjadogInitialEntities"/> class.
    /// </summary>
    public NinjadogInitialEntities()
    {
        Add(Person, new NinjadogEntity(
            new NinjadogEntityProperties
            {
                { "Id", new NinjadogEntityId() },
                { "FirstName", new NinjadogEntityProperty<string>() },
                { "LastName", new NinjadogEntityProperty<string>() },
                { "BirthDate", new NinjadogEntityProperty<DateTime>() },
            }));
    }
}
