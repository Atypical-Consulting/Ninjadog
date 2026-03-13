namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the FluentValidation validator for LoginRequest.
/// </summary>
public class LoginRequestValidatorTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "LoginRequestValidator";

    /// <inheritdoc />
    protected override bool ShouldGenerate(NinjadogAuthConfiguration? auth)
    {
        return auth is not null && auth.GenerateLoginEndpoint;
    }

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        const string fileName = "LoginRequestValidator.cs";

        var content =
            $$"""

              using FluentValidation;

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public class LoginRequestValidator : AbstractValidator<LoginRequest>
              {
                  public LoginRequestValidator()
                  {
                      RuleFor(x => x.Email).NotEmpty().EmailAddress();
                      RuleFor(x => x.Password).NotEmpty();
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
