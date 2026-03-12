namespace Ninjadog.Templates.CrudWebAPI.Template.Services;

/// <summary>
/// This template generates the service for a given entity.
/// </summary>
public sealed class ServiceTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "Service";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Services";
        var fileName = $"{st.ClassModelService}.cs";
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              using {{rootNamespace}}.Domain;
              using {{rootNamespace}}.Mapping;
              using {{rootNamespace}}.Repositories;
              using FluentValidation;
              using FluentValidation.Results;

              {{WriteFileScopedNamespace(ns)}}

              /// <summary>
              /// This service provides CRUD operations for {{st.ModelsHumanized}}.
              /// </summary>
              /// <param name="{{st.VarModelRepository}}">The repository for {{st.ModelsHumanized}}.</param>
              public partial class {{st.ClassModelService}}({{st.InterfaceModelRepository}} {{st.VarModelRepository}})
                  : {{st.InterfaceModelService}}
              {
                  /// <inheritdoc />
                  public async Task<bool> CreateAsync({{st.Model}} {{st.VarModel}})
                  {
                      var {{st.VarExistingModel}} = await {{st.VarModelRepository}}.GetAsync({{st.VarModel}}.{{entityKey.Key}});
                      if ({{st.VarExistingModel}} is not null)
                      {
                          var message = $"A {{st.ModelHumanized}} with id {{{st.VarModel}}.{{entityKey.Key}}} already exists";
                          throw new ValidationException(message, new []
                          {
                              new ValidationFailure(nameof({{st.Model}}), message)
                          });
                      }

                      var {{st.VarModelDto}} = {{st.VarModel}}.{{st.MethodToModelDto}}();
                      return await {{st.VarModelRepository}}.CreateAsync({{st.VarModelDto}});
                  }
              
                  /// <inheritdoc />
                  public async Task<{{st.Model}}?> GetAsync({{entityKey.Type}} id)
                  {
                      var {{st.VarModelDto}} = await {{st.VarModelRepository}}.GetAsync(id);
                      return {{st.VarModelDto}}?.{{st.MethodToModel}}();
                  }
              
                  /// <inheritdoc />
                  public async Task<(IEnumerable<{{st.Model}}> Items, int TotalCount)> GetAllAsync(
                      int page, int pageSize,
                      Dictionary<string, string>? filters = null,
                      string? sortBy = null,
                      bool sortDescending = false)
                  {
                      var {{st.VarModelDtos}} = await {{st.VarModelRepository}}.GetAllAsync(
                          page, pageSize, filters, sortBy, sortDescending);
                      var totalCount = await {{st.VarModelRepository}}.CountAsync(filters);
                      var items = {{st.VarModelDtos}}.Select(x => x.{{st.MethodToModel}}());
                      return (items, totalCount);
                  }
              
                  /// <inheritdoc />
                  public async Task<bool> UpdateAsync({{st.Model}} {{st.VarModel}})
                  {
                      var {{st.VarModelDto}} = {{st.VarModel}}.{{st.MethodToModelDto}}();
                      return await {{st.VarModelRepository}}.UpdateAsync({{st.VarModelDto}});
                  }
              
                  /// <inheritdoc />
                  public async Task<bool> DeleteAsync({{entityKey.Type}} id)
                  {
                      return await {{st.VarModelRepository}}.DeleteAsync(id);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
