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
        var auth = ninjadogSettings.Config.Auth;
        const string fileName = "appsettings.json";

        var content =
            $$"""
              {
                "Database": {
                  "ConnectionString": "Data Source=./{{ninjadogSettings.Config.Name}}.db"
                },
              {{GenerateJwtSection(auth)}}  "Logging": {
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

    private static string GenerateJwtSection(NinjadogAuthConfiguration? auth)
    {
        if (auth is null)
        {
            return string.Empty;
        }

        return $$"""
                   "Jwt": {
                     "Secret": "YOUR-SECRET-KEY-MIN-32-CHARACTERS-LONG-REPLACE-IN-PRODUCTION",
                     "Issuer": "{{auth.Issuer}}",
                     "Audience": "{{auth.Audience}}",
                     "ExpirationMinutes": {{auth.TokenExpirationMinutes}}
                   },

                 """;
    }
}
