namespace Ninjadog.Templates.CrudWebAPI.Template;

/// <summary>
/// This template generates the extensions class for the Template project.
/// </summary>
public class CrudWebApiExtensionsTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "CrudWebApiExtensions";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var entities = ninjadogSettings.Entities.FromKeys();
        var hasSeedData = entities.Any(e => e.SeedData is { Count: > 0 });
        var provider = ninjadogSettings.Config.DatabaseProvider;
        var factoryClassName = GetFactoryClassName(provider);
        const string fileName = "CrudWebApiExtensions.cs";

        var content =
            $$"""

              using Microsoft.AspNetCore.Diagnostics;
              using {{rootNamespace}}.Database;
              using {{rootNamespace}}.Repositories;
              using {{rootNamespace}}.Services;
              using FastEndpoints;
              using FastEndpoints.ClientGen;
              using FastEndpoints.Swagger;
              using FluentValidation;

              {{WriteFileScopedNamespace(rootNamespace)}}

              public static class NinjadogExtensions
              {
                  public static IServiceCollection AddNinjadog(
                      this IServiceCollection services,
                      ConfigurationManager config)
                  {
                      services.AddFastEndpoints();
                      services.SwaggerDocument(o =>
                      {
                          o.DocumentSettings = s =>
                          {
                              s.Title = "Ninjadog API";
                              s.Version = "v1";
                          };
                      });

                      var connectionString =
                          config.GetValue<string>("Database:ConnectionString")
                          ?? throw new InvalidOperationException("Database:ConnectionString is not configured.");

                      services.AddSingleton<IDbConnectionFactory>(_ => new {{factoryClassName}}(connectionString));
                      services.AddSingleton<DatabaseInitializer>();
              {{GenerateSeederRegistration(hasSeedData)}}
              {{GenerateModelDependenciesInjection(entities)}}
                      return services;
                  }

                  public static WebApplication UseNinjadog(this WebApplication app)
                  {
                      app.UseValidationExceptionHandler();
                      app.UseFastEndpoints();
                      app.UseSwaggerGen();

                      app.MapCSharpClientEndpoint("/cs-client", "version 1", s =>
                      {
                          s.ClassName = "ApiClient";
                          s.CSharpGeneratorSettings.Namespace = "{{rootNamespace}}.Client";
                      });

                      app.MapTypeScriptClientEndpoint("/ts-client", "version 1", s =>
                      {
                          s.ClassName = "ApiClient";
                          s.TypeScriptGeneratorSettings.Namespace = "{{rootNamespace}}.Client";
                      });

                      return app;
                  }

                  public static WebApplication UseValidationExceptionHandler(this WebApplication app)
                  {
                      app.UseExceptionHandler(errApp =>
                      {
                          errApp.Run(async ctx =>
                          {
                              var exHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();

                              if (exHandlerFeature?.Error is ValidationException exception)
                              {
                                  var validationFailureResponse = new ErrorResponse
                                  {
                                      StatusCode = 400,
                                      Message = "One or more errors occured!",
                                      Errors = exception.Errors
                                          .GroupBy(failure => failure.PropertyName)
                                          .ToDictionary(
                                              failures => failures.Key,
                                              failures => failures.Select(failure => failure.ErrorMessage).ToList())
                                  };

                                  await ctx.Response.WriteAsJsonAsync(validationFailureResponse);
                              }
                          });
                      });

                      return app;
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateSeederRegistration(bool hasSeedData)
    {
        return hasSeedData
            ? "        services.AddSingleton<DatabaseSeeder>();\n"
            : string.Empty;
    }

    private static string GenerateModelDependenciesInjection(List<NinjadogEntityWithKey> entities)
    {
        IndentedStringBuilder stringBuilder = new(2);

        foreach (var st in entities.Select(model => model.StringTokens))
        {
            stringBuilder
                .AppendLine($"services.AddSingleton<{st.InterfaceModelRepository}, {st.ClassModelRepository}>();")
                .AppendLine($"services.AddSingleton<{st.InterfaceModelService}, {st.ClassModelService}>();");
        }

        return stringBuilder.ToString();
    }

    private static string GetFactoryClassName(string provider)
    {
        return provider switch
        {
            "postgresql" => "NpgsqlConnectionFactory",
            "sqlserver" => "SqlServerConnectionFactory",
            _ => "SqliteConnectionFactory"
        };
    }
}
