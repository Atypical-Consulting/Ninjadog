using System.Reflection;

namespace Ninjadog.Templates.CrudWebAPI.UseCases;

/// <summary>
/// Loads use case settings directly from embedded ninjadog.json files,
/// eliminating duplication between JSON config and C# entity definitions.
/// </summary>
public static class UseCaseSettings
{
    private static readonly Lazy<NinjadogSettings> _todoApp =
        new(() => LoadFromEmbeddedResource("UseCases.TodoApp.ninjadog.json"));

    private static readonly Lazy<NinjadogSettings> _restaurantBooking =
        new(() => LoadFromEmbeddedResource("UseCases.RestaurantBooking.ninjadog.json"));

    /// <summary>
    /// Gets the TodoApp use case settings from its embedded ninjadog.json.
    /// </summary>
    /// <returns>The parsed <see cref="NinjadogSettings"/> for the TodoApp use case.</returns>
    public static NinjadogSettings TodoApp()
    {
        return _todoApp.Value;
    }

    /// <summary>
    /// Gets the RestaurantBooking use case settings from its embedded ninjadog.json.
    /// </summary>
    /// <returns>The parsed <see cref="NinjadogSettings"/> for the RestaurantBooking use case.</returns>
    public static NinjadogSettings RestaurantBooking()
    {
        return _restaurantBooking.Value;
    }

    private static NinjadogSettings LoadFromEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return NinjadogSettings.FromJsonString(json);
    }
}
