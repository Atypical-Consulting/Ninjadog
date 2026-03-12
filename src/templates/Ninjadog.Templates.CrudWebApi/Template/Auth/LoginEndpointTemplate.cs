namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the login endpoint (POST /api/auth/login).
/// </summary>
public class LoginEndpointTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "LoginEndpoint";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null || !auth.GenerateLoginEndpoint)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        const string fileName = "LoginEndpoint.cs";

        var content =
            $$"""

              using FastEndpoints;

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public class LoginEndpoint(IUserRepository userRepository, ITokenService tokenService)
                  : Endpoint<LoginRequest, LoginResponse>
              {
                  public override void Configure()
                  {
                      Post("/api/auth/login");
                      AllowAnonymous();
                  }

                  public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
                  {
                      var user = await userRepository.GetByEmailAsync(req.Email);

                      if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                      {
                          await Send.UnauthorizedAsync(ct);
                          return;
                      }

                      var roles = user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                      var token = tokenService.GenerateToken(user.Id, user.Email, roles);
                      var expiresAt = DateTime.UtcNow.AddMinutes({{auth.TokenExpirationMinutes}});

                      await Send.OkAsync(new LoginResponse(token, expiresAt), ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
