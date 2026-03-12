namespace Ninjadog.Templates.CrudWebAPI.Template.Services;

/// <summary>
/// This template generates the service interface for a given entity.
/// </summary>
public sealed class ServiceInterfaceTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "ServiceInterface";

    /// <inheritdoc/>
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Services";
        var fileName = $"{st.InterfaceModelService}.cs";
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              using {{rootNamespace}}.Domain;
              using FluentValidation;

              {{WriteFileScopedNamespace(ns)}}

              public partial interface {{st.InterfaceModelService}}
              {
                  /// <summary>
                  /// Asynchronously creates a new {{st.ModelHumanized}}.
                  /// </summary>
                  /// <param name="{{st.VarModel}}">The {{st.ModelHumanized}} to create.</param>
                  /// <returns>True if the creation was successful, false otherwise.</returns>
                  /// <exception cref="ValidationException">Thrown if a {{st.ModelHumanized}} with the same ID already exists.</exception>
                  Task<bool> CreateAsync({{st.Model}} {{st.VarModel}});

                  /// <summary>
                  /// Retrieves a {{st.ModelHumanized}} by its ID.
                  /// </summary>
                  /// <param name="id">The ID of the {{st.ModelHumanized}} to retrieve.</param>
                  /// <returns>The requested {{st.ModelHumanized}}, or null if not found.</returns>
                  Task<{{st.Model}}?> GetAsync({{entityKey.Type}} id);

                  /// <summary>
                  /// Retrieves a paginated, filterable, and sortable list of {{st.ModelsHumanized}}.
                  /// </summary>
                  /// <param name="page">The page number (1-based).</param>
                  /// <param name="pageSize">The number of items per page.</param>
                  /// <param name="filters">Optional dictionary of property name/value pairs to filter by.</param>
                  /// <param name="sortBy">Optional property name to sort by.</param>
                  /// <param name="sortDescending">Whether to sort in descending order.</param>
                  /// <returns>A tuple containing the items and total count.</returns>
                  Task<(IEnumerable<{{st.Model}}> Items, int TotalCount)> GetAllAsync(
                      int page, int pageSize,
                      Dictionary<string, string>? filters = null,
                      string? sortBy = null,
                      bool sortDescending = false);

                  /// <summary>
                  /// Updates an existing {{st.ModelHumanized}}.
                  /// </summary>
                  /// <param name="{{st.VarModel}}">The {{st.ModelHumanized}} to update.</param>
                  /// <returns>True if the update was successful, false otherwise.</returns>
                  Task<bool> UpdateAsync({{st.Model}} {{st.VarModel}});

                  /// <summary>
                  /// Deletes a {{st.ModelHumanized}} by its ID.
                  /// </summary>
                  /// <param name="id">The ID of the {{st.ModelHumanized}} to delete.</param>
                  /// <returns>True if the deletion was successful, false otherwise.</returns>
                  Task<bool> DeleteAsync({{entityKey.Type}} id);
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
