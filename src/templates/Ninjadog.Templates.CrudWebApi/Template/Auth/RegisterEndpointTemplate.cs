namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the register endpoint (POST /api/auth/register).
/// </summary>
public class RegisterEndpointTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "RegisterEndpoint";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null || !auth.GenerateRegisterEndpoint)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var apiVersion = ninjadogSettings.Config.Versioning?.DefaultVersion;
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
