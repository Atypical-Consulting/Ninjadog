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
              {{GenerateJwtSection(auth)}}  "Serilog": {
                  "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
                  "MinimumLevel": {
                    "Default": "Information",
                    "Override": {
                      "Microsoft.AspNetCore": "Warning",
                      "Microsoft.AspNetCore.Hosting": "Information",
                      "System": "Warning"
                    }
                  },
                  "WriteTo": [
                    {
                      "Name": "Console",
                      "Args": {
                        "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId:l} {Message:lj}{NewLine}{Exception}"
                      }
                    },
                    {
                      "Name": "File",
                      "Args": {
                        "path": "logs/log-.txt",
                        "rollingInterval": "Day",
                        "retainedFileCountLimit": 7,
                        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}"
                      }
                    }
                  ],
                  "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
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
