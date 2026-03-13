namespace Ninjadog.Templates.CrudWebAPI.Template.Validation;

/// <summary>
/// This template generates the validation rules for the update request of a given entity.
/// </summary>
public sealed class UpdateRequestValidatorTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "UpdateRequestValidator";

    /// <inheritdoc/>
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Validation";
        var fileName = $"{st.ClassUpdateModelRequestValidator}.cs";

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using FastEndpoints;
              using FluentValidation;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassUpdateModelRequestValidator}} : Validator<{{st.ClassUpdateModelRequest}}>
              {
                  public {{st.ClassUpdateModelRequestValidator}}()
                  {
              {{ValidationRuleGenerator.GenerateValidationRules(entity)}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
