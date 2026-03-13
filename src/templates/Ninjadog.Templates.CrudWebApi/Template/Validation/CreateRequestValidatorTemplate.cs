namespace Ninjadog.Templates.CrudWebAPI.Template.Validation;

/// <summary>
/// This template generates the validation rules for the create request of a given entity.
/// </summary>
public sealed class CreateRequestValidatorTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "CreateRequestValidator";

    /// <inheritdoc/>
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Validation";
        var fileName = $"{st.ClassCreateModelRequestValidator}.cs";

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using FastEndpoints;
              using FluentValidation;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassCreateModelRequestValidator}} : Validator<{{st.ClassCreateModelRequest}}>
              {
                  public {{st.ClassCreateModelRequestValidator}}()
                  {
              {{ValidationRuleGenerator.GenerateValidationRules(entity)}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
