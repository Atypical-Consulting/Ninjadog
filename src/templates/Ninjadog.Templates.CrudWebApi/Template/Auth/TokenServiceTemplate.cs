namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the TokenService class that creates JWT tokens.
/// </summary>
public class TokenServiceTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "TokenService";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        const string fileName = "TokenService.cs";

        var content =
            $$"""

              using System.IdentityModel.Tokens.Jwt;
              using System.Security.Claims;
              using System.Text;
              using Microsoft.IdentityModel.Tokens;

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public class TokenService(IConfiguration config) : ITokenService
              {
                  public string GenerateToken(string userId, string email, IEnumerable<string> roles)
                  {
                      var secret = config["Jwt:Secret"]
                          ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
                      var issuer = config["Jwt:Issuer"] ?? "{{auth.Issuer}}";
                      var audience = config["Jwt:Audience"] ?? "{{auth.Audience}}";
                      var expirationMinutes = int.TryParse(config["Jwt:ExpirationMinutes"], out var exp)
                          ? exp
                          : {{auth.TokenExpirationMinutes}};

                      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                      var claims = new List<Claim>
                      {
                          new(JwtRegisteredClaimNames.Sub, userId),
                          new(JwtRegisteredClaimNames.Email, email),
                          new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                      };

                      foreach (var role in roles)
                      {
                          claims.Add(new Claim(ClaimTypes.Role, role));
                      }

                      var token = new JwtSecurityToken(
                          issuer: issuer,
                          audience: audience,
                          claims: claims,
                          expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                          signingCredentials: credentials);

                      return new JwtSecurityTokenHandler().WriteToken(token);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
