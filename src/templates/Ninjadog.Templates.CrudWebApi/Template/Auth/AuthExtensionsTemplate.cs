using System.Globalization;
using System.Text;

namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates AuthExtensions.cs with AddJwtAuthentication() and AddAuthorizationPolicies() extension methods.
/// </summary>
public class AuthExtensionsTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "AuthExtensions";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        const string fileName = "AuthExtensions.cs";

        var content =
            $$"""

              using System.Text;
              using Microsoft.AspNetCore.Authentication.JwtBearer;
              using Microsoft.IdentityModel.Tokens;

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public static class AuthExtensions
              {
                  public static IServiceCollection AddJwtAuthentication(
                      this IServiceCollection services,
                      ConfigurationManager config)
                  {
                      var jwtSecret = config["Jwt:Secret"]
                          ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
                      var jwtIssuer = config["Jwt:Issuer"] ?? "{{auth.Issuer}}";
                      var jwtAudience = config["Jwt:Audience"] ?? "{{auth.Audience}}";

                      services.AddAuthentication(options =>
                      {
                          options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                          options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                      })
                      .AddJwtBearer(options =>
                      {
                          options.TokenValidationParameters = new TokenValidationParameters
                          {
                              ValidateIssuer = true,
                              ValidateAudience = true,
                              ValidateLifetime = true,
                              ValidateIssuerSigningKey = true,
                              ValidIssuer = jwtIssuer,
                              ValidAudience = jwtAudience,
                              IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                          };
                      });

                      return services;
                  }
              {{GenerateAuthorizationPolicies(auth.Roles)}}
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateAuthorizationPolicies(string[]? roles)
    {
        if (roles is not { Length: > 0 })
        {
            return string.Empty;
        }

        var policyLines = new StringBuilder()
            .AppendLine()
            .AppendLine("    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)")
            .AppendLine("    {")
            .AppendLine("        services.AddAuthorizationBuilder()");

        for (var i = 0; i < roles.Length; i++)
        {
            var role = roles[i];
            var separator = i < roles.Length - 1 ? string.Empty : ";";
            policyLines.AppendLine(CultureInfo.InvariantCulture, $"            .AddPolicy(\"{role}\", policy => policy.RequireRole(\"{role}\")){separator}");
        }

        return policyLines
            .AppendLine()
            .AppendLine("        return services;")
            .Append("    }")
            .ToString();
    }
}
