using System.Reflection;

namespace Ninjadog.Templates.CrudWebAPI.UseCases;

/// <summary>
/// Loads use case settings directly from embedded ninjadog.json files,
/// eliminating duplication between JSON config and C# entity definitions.
/// </summary>
public static class UseCaseSettings
{
    private static readonly Lazy<NinjadogSettings> _todoApp =
        new(() => LoadFromEmbeddedResources("UseCases.TodoApp"));

    private static readonly Lazy<NinjadogSettings> _restaurantBooking =
        new(() => LoadFromEmbeddedResources("UseCases.RestaurantBooking"));

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

    private static NinjadogSettings LoadFromEmbeddedResources(string resourcePrefix)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var jsonResourceName = $"{resourcePrefix}.ninjadog.json";

        using var stream = assembly.GetManifestResourceStream(jsonResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{jsonResourceName}' not found.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        // Extract CSV resources referenced by this use case to a temp directory
        // so the CSV seed data parser can resolve relative file paths.
        var tempDir = ExtractCsvResources(assembly, resourcePrefix);
        try
        {
            return NinjadogSettings.FromJsonString(json, tempDir);
        }
        finally
        {
            if (tempDir is not null && Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static string? ExtractCsvResources(Assembly assembly, string resourcePrefix)
    {
        var csvPrefix = $"{resourcePrefix}.";
        var csvResources = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(csvPrefix, StringComparison.Ordinal) && name.EndsWith(".csv", StringComparison.Ordinal))
            .ToList();

        if (csvResources.Count == 0)
        {
            return null;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"ninjadog-usecase-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        foreach (var resourceName in csvResources)
        {
            var fileName = resourceName[csvPrefix.Length..];
            using var resourceStream = assembly.GetManifestResourceStream(resourceName)!;
            var filePath = Path.Combine(tempDir, fileName);
            using var fileStream = File.Create(filePath);
            resourceStream.CopyTo(fileStream);
        }

        return tempDir;
    }
}
