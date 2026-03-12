namespace Ninjadog.Templates.CrudWebAPI.Template;

/// <summary>
/// This template generates the Properties/launchSettings.json file for the Template project.
/// It configures the development server to open Swagger UI on launch.
/// </summary>
public class LaunchSettingsTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "LaunchSettings";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        const string fileName = "launchSettings.json";

        var content =
            """
            {
              "$schema": "https://json.schemastore.org/launchsettings.json",
              "profiles": {
                "http": {
                  "commandName": "Project",
                  "dotnetRunMessages": true,
                  "launchBrowser": true,
                  "launchUrl": "swagger",
                  "applicationUrl": "http://localhost:5084",
                  "environmentVariables": {
                    "ASPNETCORE_ENVIRONMENT": "Development"
                  }
                },
                "https": {
                  "commandName": "Project",
                  "dotnetRunMessages": true,
                  "launchBrowser": true,
                  "launchUrl": "swagger",
                  "applicationUrl": "https://localhost:7272;http://localhost:5084",
                  "environmentVariables": {
                    "ASPNETCORE_ENVIRONMENT": "Development"
                  }
                }
              }
            }
            """;

        return CreateNinjadogContentFile(fileName, content, false);
    }
}
