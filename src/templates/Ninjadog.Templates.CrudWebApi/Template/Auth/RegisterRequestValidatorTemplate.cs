namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the FluentValidation validator for RegisterRequest.
/// </summary>
public class RegisterRequestValidatorTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "RegisterRequestValidator";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null || !auth.GenerateRegisterEndpoint)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
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
