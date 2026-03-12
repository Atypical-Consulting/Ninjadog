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
        var aot = ninjadogSettings.Config.Aot;
        var auth = ninjadogSettings.Config.Auth;
        var hasSeedData = ninjadogSettings.Entities.FromKeys().Any(e => e.SeedData is { Count: > 0 });
        const string fileName = "Program.cs";

        var builderCall = aot
            ? "WebApplication.CreateSlimBuilder(args)"
            : "WebApplication.CreateBuilder(args)";

        var content =
            $$"""

              using {{rootNamespace}};
              using {{rootNamespace}}.Database;
              using {{rootNamespace}}.Middleware;
              {{GenerateAuthUsing(auth, rootNamespace)}}using Serilog;

              Log.Logger = new LoggerConfiguration()
                  .WriteTo.Console()
                  .CreateBootstrapLogger();

              try
              {
                  const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

                  var builder = {{builderCall}};
                  var services = builder.Services;
                  var config = builder.Configuration;

                  builder.Host.UseSerilog((context, loggerConfig) =>
                      loggerConfig.ReadFrom.Configuration(context.Configuration));
              {{GenerateAotJsonOptions(aot, rootNamespace)}}
                  services.AddCors(options =>
                  {
                      options.AddPolicy(name: myAllowSpecificOrigins,
                          policy =>
                          {
                              {{GenerateCorsPolicy(cors)}}
                          });
                  });
              {{GenerateAuthServiceRegistration(auth)}}
                  services.AddNinjadog(config);

                  var app = builder.Build();

                  app.UseMiddleware<RequestCorrelationMiddleware>();
                  app.UseSerilogRequestLogging();
                  app.UseCors(myAllowSpecificOrigins);
              {{GenerateAuthMiddleware(auth)}}    app.UseNinjadog();

                  await app.Services
                      .GetRequiredService<DatabaseInitializer>()
                      .InitializeAsync()
                      .ConfigureAwait(false);
              {{GenerateUserInitializerCall(auth)}}{{GenerateSeederCall(hasSeedData)}}
                  app.Run();
              }
              catch (Exception ex)
              {
                  Log.Fatal(ex, "Application terminated unexpectedly");
              }
              finally
              {
                  await Log.CloseAndFlushAsync();
              }
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

    private static string GenerateAotJsonOptions(bool aot, string rootNamespace)
    {
        return !aot
            ? string.Empty
            : $$"""

                  services.ConfigureHttpJsonOptions(options =>
                  {
                      options.SerializerOptions.TypeInfoResolverChain.Insert(0, {{rootNamespace}}.AppJsonSerializerContext.Default);
                  });
              """;
    }

    private static string GenerateAuthUsing(NinjadogAuthConfiguration? auth, string rootNamespace)
    {
        return auth is null
            ? string.Empty
            : $"using {rootNamespace}.Auth;\n";
    }

    private static string GenerateAuthServiceRegistration(NinjadogAuthConfiguration? auth)
    {
        if (auth is null)
        {
            return string.Empty;
        }

        var result = "\n    services.AddJwtAuthentication(config);";

        if (auth.Roles is { Length: > 0 })
        {
            result += "\n    services.AddAuthorizationPolicies();";
        }

        return result + "\n";
    }

    private static string GenerateAuthMiddleware(NinjadogAuthConfiguration? auth)
    {
        return auth is null
            ? string.Empty
            : """
                  app.UseAuthentication();
                  app.UseAuthorization();

              """;
    }

    private static string GenerateUserInitializerCall(NinjadogAuthConfiguration? auth)
    {
        return auth is null
            ? string.Empty
            : """

                  await app.Services
                      .GetRequiredService<UserInitializer>()
                      .InitializeAsync()
                      .ConfigureAwait(false);
              """;
    }

    private static string GenerateSeederCall(bool hasSeedData)
    {
        return !hasSeedData
            ? string.Empty
            : """

                  await app.Services
                      .GetRequiredService<DatabaseSeeder>()
                      .SeedAsync()
                      .ConfigureAwait(false);
              """;
    }
}
