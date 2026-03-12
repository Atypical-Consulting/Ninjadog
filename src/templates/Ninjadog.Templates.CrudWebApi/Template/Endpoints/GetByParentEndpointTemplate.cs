namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates nested GET endpoints for child entities under their parent.
/// For example, GET /parents/{parentId}/children.
/// </summary>
public sealed class GetByParentEndpointTemplate
    : NinjadogTemplate
{
    private int? _apiVersion;

    /// <inheritdoc />
    public override string Name => "GetByParentEndpoint";

    /// <inheritdoc />
    public override IEnumerable<NinjadogContentFile> GenerateMany(NinjadogSettings ninjadogSettings)
    {
        _apiVersion = ninjadogSettings.Config.Versioning?.DefaultVersion;
        var entities = ninjadogSettings.Entities.FromKeys();

        foreach (var entity in entities)
        {
            if (entity.Relationships == null)
            {
                continue;
            }

            foreach (var (relationName, relationship) in entity.Relationships)
            {
                if (relationship.RelationshipType != NinjadogEntityRelationshipType.OneToMany)
                {
                    continue;
                }

                var childEntity = entities.FirstOrDefault(e => e.Key == relationship.RelatedEntity);
                if (childEntity == null)
                {
                    continue;
                }

                yield return GenerateNestedEndpoint(entity, childEntity, ninjadogSettings.Config.RootNamespace);
            }
        }
    }

    private NinjadogContentFile GenerateNestedEndpoint(
        NinjadogEntityWithKey parentEntity,
        NinjadogEntityWithKey childEntity,
        string rootNamespace)
    {
        var parentSt = parentEntity.StringTokens;
        var childSt = childEntity.StringTokens;
        var parentKey = parentEntity.Properties.GetEntityKey();
        var ns = $"{rootNamespace}.Endpoints";
        var className = $"Get{childSt.Models}By{parentSt.Model}Endpoint";
        var fileName = Path.Combine(childSt.Model, $"{className}.cs");
        var routeConstraint = GetRouteConstraint(parentKey.Type);
        var routeConstraintStr = string.IsNullOrEmpty(routeConstraint) ? string.Empty : $":{routeConstraint}";

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Mapping;
              using {{rootNamespace}}.Services;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{className}}({{childSt.InterfaceModelService}} {{childSt.VarModelService}})
                  : EndpointWithoutRequest<{{childSt.ClassGetAllModelsResponse}}>
              {
                  public override void Configure()
                  {
                      Get("{{parentSt.ModelEndpoint}}/{{{parentSt.VarModel}}Id{{routeConstraintStr}}}{{childSt.ModelEndpoint}}");
                      AllowAnonymous();{{GenerateVersionCall(_apiVersion)}}
                  }

                  public override async Task HandleAsync(CancellationToken ct)
                  {
                      var {{parentSt.VarModel}}Id = Route<string>("{{parentSt.VarModel}}Id");
                      var page = int.TryParse(HttpContext.Request.Query["page"], out var p) && p > 0 ? p : 1;
                      var pageSize = int.TryParse(HttpContext.Request.Query["pageSize"], out var ps) && ps > 0 ? ps : 10;

                      var ({{childSt.VarModels}}, totalCount) = await {{childSt.VarModelService}}.GetAllAsync(page, pageSize);
                      var {{childSt.VarModelsResponse}} = {{childSt.VarModels}}.{{childSt.MethodToModelsResponse}}(page, pageSize, totalCount);
                      await Send.OkAsync({{childSt.VarModelsResponse}}, ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
