namespace Ninjadog.Templates.CrudWebAPI.Setup;

/// <summary>
/// Represents the manifest for the CrudWebAPI template.
/// </summary>
public class CrudTemplateManifest : NinjadogTemplateManifest
{
    /// <inheritdoc />
    public override string Name { get; init; } = "CrudWebAPI";

    /// <inheritdoc />
    public override string Description { get; init; } = "Your ASP.NET Core Web API project with CRUD operations";

    /// <inheritdoc />
    public override string Version { get; init; } = "1.0.0-alpha.2";

    /// <inheritdoc />
    public override string Author { get; init; } = "Philippe Matray";

    /// <inheritdoc />
    public override string License { get; init; } = "All rights reserved";

    /// <inheritdoc />
    public override List<string> NuGetPackages { get; init; } =
    [
        "Dapper:2.1.72",
        "FastEndpoints:5.32.0",
        "FastEndpoints.ClientGen:5.32.0",
        "FastEndpoints.Swagger:5.32.0",
        "FluentValidation:12.1.1",
        "ValueOf:2.0.31",
        "Microsoft.Data.Sqlite:10.0.4",
        "Npgsql:9.0.3",
        "Microsoft.Data.SqlClient:6.0.1"
    ];

    /// <inheritdoc />
    public override NinjadogTemplates Templates { get; init; } = new CrudTemplates();
}
