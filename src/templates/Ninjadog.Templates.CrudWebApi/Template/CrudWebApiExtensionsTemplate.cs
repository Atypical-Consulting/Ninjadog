using Ninjadog.Templates.CrudWebAPI.Template.Database;

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
        var hasAuth = ninjadogSettings.Config.Auth is not null;
        var provider = ninjadogSettings.Config.DatabaseProvider;
        var aot = ninjadogSettings.Config.Aot;
        var versioning = ninjadogSettings.Config.Versioning;
        var factoryClassName = GetFactoryClassName(provider);
        var projectName = ninjadogSettings.Config.Name;
        var projectVersion = ninjadogSettings.Config.Version;
        var projectDescription = ninjadogSettings.Config.Description;
        const string fileName = "CrudWebApiExtensions.cs";

        var content = aot
            ? GenerateAotContent(rootNamespace, entities, hasSeedData, hasAuth, factoryClassName, versioning)
            : GenerateStandardContent(rootNamespace, entities, hasSeedData, hasAuth, factoryClassName, projectName, projectVersion, projectDescription, versioning);

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateStandardContent(
        string rootNamespace,
        List<NinjadogEntityWithKey> entities,
        bool hasSeedData,
        bool hasAuth,
        string factoryClassName,
        string projectName,
        string projectVersion,
        string projectDescription,
        NinjadogVersioningConfiguration? versioning)
    {
        return
            $$"""

              using Microsoft.AspNetCore.Diagnostics;
              using Microsoft.AspNetCore.Mvc;
              using {{rootNamespace}}.Database;
              using {{rootNamespace}}.Repositories;
              using {{rootNamespace}}.Services;
              {{GenerateAuthUsing(hasAuth, rootNamespace)}}using FastEndpoints;
              using FastEndpoints.ClientGen;
              using FastEndpoints.Swagger;
              using FluentValidation;

              {{WriteFileScopedNamespace(rootNamespace)}}

              public static class NinjadogExtensions
              {
                  private const string ProblemJsonContentType = "application/problem+json";

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

              {{GenerateConnectionAndDependencies(entities, hasSeedData, hasAuth, factoryClassName)}}
                      return services;
                  }

                  public static WebApplication UseNinjadog(this WebApplication app)
                  {
                      app.UseGlobalExceptionHandler();
                      app.UseDefaultFiles();
                      app.UseStaticFiles();
              {{GenerateUseFastEndpoints(versioning)}}
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

              {{GenerateGlobalExceptionHandler(aot: false)}}
              }
              """;
    }

    private static string GenerateAotContent(
        string rootNamespace, List<NinjadogEntityWithKey> entities, bool hasSeedData, bool hasAuth, string factoryClassName, NinjadogVersioningConfiguration? versioning)
    {
        return
            $$"""

              using System.Text.Json;
              using Microsoft.AspNetCore.Diagnostics;
              using Microsoft.AspNetCore.Mvc;
              using {{rootNamespace}}.Database;
              using {{rootNamespace}}.Repositories;
              using {{rootNamespace}}.Services;
              {{GenerateAuthUsing(hasAuth, rootNamespace)}}using FastEndpoints;
              using FluentValidation;

              {{WriteFileScopedNamespace(rootNamespace)}}

              public static class NinjadogExtensions
              {
                  private const string ProblemJsonContentType = "application/problem+json";

                  public static IServiceCollection AddNinjadog(
                      this IServiceCollection services,
                      ConfigurationManager config)
                  {
                      services.AddFastEndpoints(o =>
                      {
                          o.SerializerContext = AppJsonSerializerContext.Default;
                      });

              {{GenerateConnectionAndDependencies(entities, hasSeedData, hasAuth, factoryClassName)}}
                      return services;
                  }

                  public static WebApplication UseNinjadog(this WebApplication app)
                  {
                      app.UseGlobalExceptionHandler();
              {{GenerateUseFastEndpoints(versioning)}}

                      return app;
                  }

              {{GenerateGlobalExceptionHandler(aot: true)}}
              }
              """;
    }

    private static string GenerateAuthUsing(bool hasAuth, string rootNamespace)
    {
        return hasAuth
            ? $"using {rootNamespace}.Auth;\n"
            : string.Empty;
    }

    private static string GenerateConnectionAndDependencies(
        List<NinjadogEntityWithKey> entities, bool hasSeedData, bool hasAuth, string factoryClassName)
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

        if (hasAuth)
        {
            sb.AppendLine("services.AddSingleton<ITokenService, TokenService>();")
                .AppendLine("services.AddScoped<IUserRepository, UserRepository>();")
                .AppendLine("services.AddSingleton<UserInitializer>();");
        }

        foreach (var st in entities.Select(model => model.StringTokens))
        {
            sb.AppendLine($"services.AddScoped<{st.InterfaceModelRepository}, {st.ClassModelRepository}>();")
                .AppendLine($"services.AddScoped<{st.InterfaceModelService}, {st.ClassModelService}>();");
        }

        return sb.ToString();
    }

    private static string GenerateGlobalExceptionHandler(bool aot)
    {
        var writeAsJsonCall = aot
            ? "await ctx.Response.WriteAsJsonAsync(problemDetails, AppJsonSerializerContext.Default.ProblemDetails, ctx.RequestAborted);"
            : "await ctx.Response.WriteAsJsonAsync(problemDetails, ctx.RequestAborted);";

        return
            $$"""
                  private static WebApplication UseGlobalExceptionHandler(this WebApplication app)
                  {
                      app.UseExceptionHandler(errApp =>
                      {
                          errApp.Run(async ctx =>
                          {
                              var exHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                              var error = exHandlerFeature?.Error;

                              int statusCode;
                              Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails;

                              if (error is ValidationException validationException)
                              {
                                  statusCode = StatusCodes.Status400BadRequest;
                                  problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                                  {
                                      Status = statusCode,
                                      Title = "One or more validation errors occurred.",
                                      Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                                      Detail = "See the errors property for details.",
                                      Extensions =
                                      {
                                          ["errors"] = validationException.Errors
                                              .GroupBy(f => f.PropertyName)
                                              .ToDictionary(
                                                  g => g.Key,
                                                  g => g.Select(f => f.ErrorMessage).ToArray())
                                      }
                                  };
                              }
                              else
                              {
                                  statusCode = StatusCodes.Status500InternalServerError;
                                  problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                                  {
                                      Status = statusCode,
                                      Title = "An unexpected error occurred.",
                                      Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
                                  };
                              }

                              ctx.Response.StatusCode = statusCode;
                              ctx.Response.ContentType = ProblemJsonContentType;
                              {{writeAsJsonCall}}
                          });
                      });

                      return app;
                  }
              """;
    }

    private static string GenerateUseFastEndpoints(NinjadogVersioningConfiguration? versioning)
    {
        if (versioning is null)
        {
            return "        app.UseFastEndpoints();";
        }

        var prepend = versioning.IsUrlPath ? "true" : "false";

        return $$"""
                     app.UseFastEndpoints(c =>
                     {
                         c.Versioning.Prefix = "{{versioning.Prefix}}";
                         c.Versioning.DefaultVersion = {{versioning.DefaultVersion}};
                         c.Versioning.PrependToRoute = {{prepend}};
                     });
             """;
    }

    private static string GetFactoryClassName(string provider)
    {
        return DatabaseProviderHelper.GetConnectionFactoryDetails(provider).ClassName;
    }
}
