namespace Ninjadog.Templates.CrudWebAPI.Template.Repositories;

/// <summary>
/// This template generates the repository interface for a given entity.
/// </summary>
public sealed class RepositoryInterfaceTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "RepositoryInterface";

    /// <inheritdoc/>
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Repositories";
        var fileName = $"{st.InterfaceModelRepository}.cs";
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Data;

              {{WriteFileScopedNamespace(ns)}}

              public partial interface {{st.InterfaceModelRepository}}
              {
                  Task<bool> CreateAsync({{st.ClassModelDto}} {{st.VarModel}});

                  Task<{{st.ClassModelDto}}?> GetAsync({{entityKey.Type}} id);

                  Task<IEnumerable<{{st.ClassModelDto}}>> GetAllAsync(int page, int pageSize);

                  Task<int> CountAsync();

                  Task<bool> UpdateAsync({{st.ClassModelDto}} {{st.VarModel}});

                  Task<bool> DeleteAsync({{entityKey.Type}} id);
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
