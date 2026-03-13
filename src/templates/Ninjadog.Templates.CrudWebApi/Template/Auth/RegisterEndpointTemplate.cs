namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the register endpoint (POST /api/auth/register).
/// </summary>
public class RegisterEndpointTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "RegisterEndpoint";

    /// <inheritdoc />
    protected override bool ShouldGenerate(NinjadogAuthConfiguration? auth)
    {
        return auth is not null && auth.GenerateRegisterEndpoint;
    }

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        var apiVersion = settings.Config.Versioning?.DefaultVersion;
        const string fileName = "RegisterEndpoint.cs";

        var content =
            $$"""

              using FastEndpoints;

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public class RegisterEndpoint(IUserRepository userRepository, ITokenService tokenService)
                  : Endpoint<RegisterRequest, RegisterResponse>
              {
                  public override void Configure()
                  {
                      Post("/api/auth/register");
                      AllowAnonymous();{{GenerateVersionCall(apiVersion)}}
                  }

                  public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
                  {
                      if (await userRepository.ExistsAsync(req.Email))
                      {
                          AddError("Email is already registered.");
                          await Send.ErrorsAsync(cancellation: ct);
                          return;
                      }

                      var user = new User
                      {
                          Email = req.Email,
                          PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
                      };

                      await userRepository.CreateAsync(user);

                      await Send.CreatedAtAsync<LoginEndpoint>(
                          routeValues: null,
                          responseBody: new RegisterResponse(user.Id, user.Email),
                          generateAbsoluteUrl: true,
                          cancellation: ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
