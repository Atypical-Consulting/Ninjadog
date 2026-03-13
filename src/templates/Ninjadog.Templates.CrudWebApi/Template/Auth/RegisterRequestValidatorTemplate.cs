namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the FluentValidation validator for RegisterRequest.
/// </summary>
public class RegisterRequestValidatorTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "RegisterRequestValidator";

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
        const string fileName = "RegisterRequestValidator.cs";

        var content =
            $$"""

              using FluentValidation;

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
              {
                  public RegisterRequestValidator()
                  {
                      RuleFor(x => x.Email).NotEmpty().EmailAddress();
                      RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
                      RuleFor(x => x.ConfirmPassword)
                          .NotEmpty()
                          .Equal(x => x.Password)
                          .WithMessage("Passwords do not match.");
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
