// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

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
        const string fileName = "CrudWebApiExtensions.cs";

        var content =
            $$"""

              using Microsoft.AspNetCore.Diagnostics;
              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Database;
              using {{rootNamespace}}.Mapping;
              using {{rootNamespace}}.Repositories;
              using {{rootNamespace}}.Services;
              using {{rootNamespace}}.Validation;
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

                      services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
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
        if (!hasSeedData)
        {
            return string.Empty;
        }

        return "        services.AddSingleton<DatabaseSeeder>();\n";
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
}
