// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Templates.CrudWebAPI.Template;

/// <summary>
/// This template generates the Program.cs file for the Template project.
/// </summary>
public class ProgramTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "Program";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var cors = ninjadogSettings.Config.Cors;
        var hasSeedData = ninjadogSettings.Entities.FromKeys().Any(e => e.SeedData is { Count: > 0 });
        const string fileName = "Program.cs";

        var content =
            $$"""

              using {{rootNamespace}};
              using {{rootNamespace}}.Database;

              const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

              var builder = WebApplication.CreateBuilder(args);
              var services = builder.Services;
              var config = builder.Configuration;

              services.AddCors(options =>
              {
                  options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          {{GenerateCorsPolicy(cors)}}
                      });
              });

              services.AddNinjadog(config);

              var app = builder.Build();

              app.UseCors(myAllowSpecificOrigins);
              app.UseNinjadog();

              await app.Services
                  .GetRequiredService<DatabaseInitializer>()
                  .InitializeAsync()
                  .ConfigureAwait(false);
              {{GenerateSeederCall(hasSeedData)}}
              app.Run();
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    /// <summary>
    /// Generates the CORS policy body based on the provided configuration.
    /// When no configuration is provided, falls back to allowing localhost:7270.
    /// </summary>
    /// <param name="cors">The optional CORS configuration.</param>
    /// <returns>The CORS policy builder chain as a string.</returns>
    private static string GenerateCorsPolicy(NinjadogCorsConfiguration? cors)
    {
        var origins = cors?.Origins ?? ["https://localhost:7270"];
        var originsStr = string.Join(", ", origins.Select(o => $"\"{o}\""));

        // The first line is positioned by the {{...}} interpolation in the template.
        // Continuation lines need 10 spaces to align with the first line's position.
        const string continuation = "\n          ";

        var result = $"policy.WithOrigins({originsStr})";

        if (cors?.Methods is { Length: > 0 })
        {
            var methodsStr = string.Join(", ", cors.Methods.Select(m => $"\"{m}\""));
            result += $"{continuation}    .WithMethods({methodsStr})";
        }

        if (cors?.Headers is { Length: > 0 })
        {
            var headersStr = string.Join(", ", cors.Headers.Select(h => $"\"{h}\""));
            result += $"{continuation}    .WithHeaders({headersStr})";
        }

        return result + ";";
    }

    private static string GenerateSeederCall(bool hasSeedData)
    {
        if (!hasSeedData)
        {
            return string.Empty;
        }

        return """

              await app.Services
                  .GetRequiredService<DatabaseSeeder>()
                  .SeedAsync()
                  .ConfigureAwait(false);
              """;
    }
}
