namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the FluentValidation validator for LoginRequest.
/// </summary>
public class LoginRequestValidatorTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "LoginRequestValidator";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null || !auth.GenerateLoginEndpoint)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
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
