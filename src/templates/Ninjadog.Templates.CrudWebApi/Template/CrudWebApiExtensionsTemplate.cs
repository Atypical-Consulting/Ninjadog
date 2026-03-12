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
        var aot = ninjadogSettings.Config.Aot;
        var factoryClassName = GetFactoryClassName(provider);
        var projectName = ninjadogSettings.Config.Name;
        var projectVersion = ninjadogSettings.Config.Version;
        var projectDescription = ninjadogSettings.Config.Description;
        const string fileName = "CrudWebApiExtensions.cs";

        var content = aot
            ? GenerateAotContent(rootNamespace, entities, hasSeedData, factoryClassName)
            : GenerateStandardContent(rootNamespace, entities, hasSeedData, factoryClassName, projectName, projectVersion, projectDescription);

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateStandardContent(
        string rootNamespace,
        List<NinjadogEntityWithKey> entities,
        bool hasSeedData,
        string factoryClassName,
        string projectName,
        string projectVersion,
        string projectDescription)
    {
        return
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
                              s.Title = "{{projectName}}";
                              s.Version = "v{{projectVersion}}";
                              s.Description = "{{projectDescription}}";
                          };
                      });

              {{GenerateConnectionAndDependencies(entities, hasSeedData, factoryClassName)}}
                      return services;
                  }

                  public static WebApplication UseNinjadog(this WebApplication app)
                  {
                      app.UseValidationExceptionHandler();
                      app.UseDefaultFiles();
                      app.UseStaticFiles();
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

              {{GenerateValidationExceptionHandler(aot: false)}}
              }
              """;
    }

    private static string GenerateAotContent(
        string rootNamespace, List<NinjadogEntityWithKey> entities, bool hasSeedData, string factoryClassName)
    {
        return
            $$"""

              using System.Text.Json;
              using Microsoft.AspNetCore.Diagnostics;
              using {{rootNamespace}}.Database;
              using {{rootNamespace}}.Repositories;
              using {{rootNamespace}}.Services;
              using FastEndpoints;
              using FluentValidation;

              {{WriteFileScopedNamespace(rootNamespace)}}

              public static class NinjadogExtensions
              {
                  public static IServiceCollection AddNinjadog(
                      this IServiceCollection services,
                      ConfigurationManager config)
                  {
                      services.AddFastEndpoints(o =>
                      {
                          o.SerializerContext = AppJsonSerializerContext.Default;
                      });

              {{GenerateConnectionAndDependencies(entities, hasSeedData, factoryClassName)}}
                      return services;
                  }

                  public static WebApplication UseNinjadog(this WebApplication app)
                  {
                      app.UseValidationExceptionHandler();
                      app.UseFastEndpoints();

                      return app;
                  }

              {{GenerateValidationExceptionHandler(aot: true)}}
              }
              """;
    }

    private static string GenerateConnectionAndDependencies(
        List<NinjadogEntityWithKey> entities, bool hasSeedData, string factoryClassName)
    {
        IndentedStringBuilder sb = new(2);

        sb.AppendLine("var connectionString =")
            .AppendLine("    config.GetValue<string>(\"Database:ConnectionString\")")
            .AppendLine("    ?? throw new InvalidOperationException(\"Database:ConnectionString is not configured.\");")
            .AppendLine()
            .AppendLine($"services.AddSingleton<IDbConnectionFactory>(_ => new {factoryClassName}(connectionString));")
            .AppendLine("services.AddSingleton<DatabaseInitializer>();");

        if (hasSeedData)
        {
            sb.AppendLine("services.AddSingleton<DatabaseSeeder>();");
        }

        foreach (var st in entities.Select(model => model.StringTokens))
        {
            sb.AppendLine($"services.AddSingleton<{st.InterfaceModelRepository}, {st.ClassModelRepository}>();")
                .AppendLine($"services.AddSingleton<{st.InterfaceModelService}, {st.ClassModelService}>();");
        }

        return sb.ToString();
    }

    private static string GenerateValidationExceptionHandler(bool aot)
    {
        var writeAsJsonCall = aot
            ? "await ctx.Response.WriteAsJsonAsync(validationFailureResponse, AppJsonSerializerContext.Default.ErrorResponse);"
            : "await ctx.Response.WriteAsJsonAsync(validationFailureResponse);";

        return
            $$"""
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

                                  {{writeAsJsonCall}}
                              }
                          });
                      });

                      return app;
                  }
              """;
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
