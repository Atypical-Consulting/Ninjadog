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
        var rateLimit = ninjadogSettings.Config.RateLimit;
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
              {{GenerateRateLimitUsing(rateLimit)}}{{GenerateAuthUsing(auth, rootNamespace)}}using Serilog;

              Log.Logger = new LoggerConfiguration()
                  .WriteTo.Console()
                  .CreateBootstrapLogger();

              try
              {
              {{GenerateCorsConstant(cors)}}    var builder = {{builderCall}};
                  var services = builder.Services;
                  var config = builder.Configuration;

                  builder.Host.UseSerilog((context, loggerConfig) =>
                      loggerConfig.ReadFrom.Configuration(context.Configuration));
              {{GenerateAotJsonOptions(aot, rootNamespace)}}{{GenerateCorsServiceRegistration(cors)}}
              {{GenerateAuthServiceRegistration(auth)}}{{GenerateRateLimitServiceRegistration(rateLimit)}}
                  services.AddNinjadog(config);

                  var app = builder.Build();

                  app.UseMiddleware<RequestCorrelationMiddleware>();
                  app.UseSerilogRequestLogging();
              {{GenerateCorsMiddleware(cors)}}{{GenerateRateLimitMiddleware(rateLimit)}}{{GenerateAuthMiddleware(auth)}}    app.UseNinjadog();

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

    private static string GenerateCorsConstant(NinjadogCorsConfiguration? cors)
    {
        return cors is null
            ? string.Empty
            : """
                  const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

              """;
    }

    private static string GenerateCorsServiceRegistration(NinjadogCorsConfiguration? cors)
    {
        if (cors is null)
        {
            return string.Empty;
        }

        var originsStr = string.Join(", ", cors.Origins.Select(o => $"\"{o}\""));

        var policyBuilder = $"policy.WithOrigins({originsStr})";

        if (cors.Methods is { Length: > 0 })
        {
            var methodsStr = string.Join(", ", cors.Methods.Select(m => $"\"{m}\""));
            policyBuilder += $"\n                  .WithMethods({methodsStr})";
        }

        if (cors.Headers is { Length: > 0 })
        {
            var headersStr = string.Join(", ", cors.Headers.Select(h => $"\"{h}\""));
            policyBuilder += $"\n                  .WithHeaders({headersStr})";
        }

        return $$"""

                  services.AddCors(options =>
                  {
                      options.AddPolicy(name: myAllowSpecificOrigins,
                          policy =>
                          {
                              {{policyBuilder}};
                          });
                  });
              """;
    }

    private static string GenerateCorsMiddleware(NinjadogCorsConfiguration? cors)
    {
        return cors is null
            ? string.Empty
            : """
                  app.UseCors(myAllowSpecificOrigins);

              """;
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

    private static string GenerateRateLimitUsing(NinjadogRateLimitConfiguration? rateLimit)
    {
        return rateLimit is null
            ? string.Empty
            : """
              using System.Threading.RateLimiting;
              using Microsoft.AspNetCore.RateLimiting;

              """;
    }

    private static string GenerateRateLimitServiceRegistration(NinjadogRateLimitConfiguration? rateLimit)
    {
        if (rateLimit is null)
        {
            return string.Empty;
        }

        return $$"""

                  services.AddRateLimiter(options =>
                  {
                      options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                      options.AddSlidingWindowLimiter("sliding", limiter =>
                      {
                          limiter.PermitLimit = {{rateLimit.PermitLimit}};
                          limiter.Window = TimeSpan.FromSeconds({{rateLimit.WindowSeconds}});
                          limiter.SegmentsPerWindow = {{rateLimit.SegmentsPerWindow}};
                          limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                          limiter.QueueLimit = 0;
                      });
                  });
              """;
    }

    private static string GenerateRateLimitMiddleware(NinjadogRateLimitConfiguration? rateLimit)
    {
        return rateLimit is null
            ? string.Empty
            : """
                  app.UseRateLimiter();

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
