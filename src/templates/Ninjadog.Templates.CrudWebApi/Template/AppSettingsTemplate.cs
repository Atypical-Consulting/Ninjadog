namespace Ninjadog.Templates.CrudWebAPI.Template;

/// <summary>
/// This template generates the appsettings.json file for the Template project.
/// </summary>
public class AppSettingsTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "appsettings";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        const string fileName = "appsettings.json";

        var content =
            $$"""
              {
                "Database": {
                  "ConnectionString": "Data Source=./{{ninjadogSettings.Config.Name}}.db"
                },
                "Logging": {
                  "LogLevel": {
                    "Default": "Information",
                    "Microsoft.AspNetCore": "Warning"
                  }
                },
                "AllowedHosts": "*"
              }
              """;

        return CreateNinjadogContentFile(fileName, content, false);
    }
}
