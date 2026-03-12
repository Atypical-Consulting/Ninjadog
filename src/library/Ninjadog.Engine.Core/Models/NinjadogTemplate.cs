namespace Ninjadog.Engine.Core.Models;

/// <summary>
/// Represents an abstract base class for Ninjadog templates.
/// This class provides the foundational structure for generating template-based code.
/// </summary>
public abstract class NinjadogTemplate
{
    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets or sets the category of the template.
    /// </summary>
    public virtual string? Category { get; set; }

    /// <summary>
    /// Generates code in a single file based on the provided settings.
    /// This method can be overridden by derived classes to implement specific logic
    /// for one-to-one code generation.
    /// </summary>
    /// <param name="ninjadogSettings">The settings to be used for code generation.</param>
    /// <returns>A string representing the generated code, or null if not implemented.</returns>
    public virtual NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        // Default implementation: return null
        // Override this method in derived classes to implement specific logic
        // for generating code in a single file.
        return NinjadogContentFile.Empty;
    }

    /// <summary>
    /// Generates content for a single entity. This method can be overridden in derived classes
    /// to provide specific generation logic for each entity.
    /// </summary>
    /// <param name="entity">The entity for which to generate content.</param>
    /// <param name="rootNamespace">The root namespace to be used in content generation.</param>
    /// <returns>A string representing the generated content for the specified entity.</returns>
    public virtual NinjadogContentFile GenerateOneByEntity(NinjadogEntityWithKey entity, string rootNamespace)
    {
        // Default implementation for generating content for a single entity.
        // This can be overridden in derived classes for specific entity generation logic.
        return NinjadogContentFile.Empty;
    }

    /// <summary>
    /// Generates code in multiple files based on the provided settings.
    /// This method iterates over entities and generates content for each.
    /// It can be overridden by derived classes to implement specific logic
    /// for one-to-many code generation.
    /// </summary>
    /// <param name="ninjadogSettings">The settings to be used for code generation.</param>
    /// <returns>An enumerable of strings, each representing generated code for an entity.</returns>
    public virtual IEnumerable<NinjadogContentFile> GenerateMany(NinjadogSettings ninjadogSettings)
    {
        var entities = ninjadogSettings.Entities.FromKeys();
        var rootNs = ninjadogSettings.Config.RootNamespace;

        foreach (var entity in entities)
        {
            yield return GenerateOneByEntity(entity, rootNs);
        }
    }

    /// <summary>
    /// Generates a file-scoped namespace declaration based on the provided namespace string.
    /// This method creates a concise namespace declaration that applies to the entire source file,
    /// allowing for a more streamlined and modern code layout.
    /// </summary>
    /// <param name="ns">The namespace to be used in the declaration. If null, no declaration is generated.</param>
    /// <returns>A file-scoped namespace declaration, or null if the input namespace is null.</returns>
    protected static string? WriteFileScopedNamespace(string? ns)
    {
        return ns is not null
            ? $"namespace {ns};"
            : null;
    }

    /// <summary>
    /// Gets the FastEndpoints route constraint string for a given type name.
    /// </summary>
    /// <param name="typeName">The CLR type name (e.g., "Guid", "Int32").</param>
    /// <returns>The corresponding route constraint string.</returns>
    protected static string GetRouteConstraint(string typeName)
    {
        return typeName switch
        {
            "Guid" => "guid",
            "Int32" => "int",
            "Int64" => "long",
            "String" => "alpha",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Generates a FastEndpoints Version() call for the given API version.
    /// Returns an empty string when no version is specified.
    /// </summary>
    /// <param name="apiVersion">The optional API version number.</param>
    /// <returns>A Version() call string with leading newline and indentation, or empty.</returns>
    protected static string GenerateVersionCall(int? apiVersion)
    {
        return apiVersion.HasValue
            ? $"\n        Version({apiVersion.Value});"
            : string.Empty;
    }

    /// <summary>
    /// Creates a new instance of <see cref="NinjadogContentFile"/> with the specified file name and content.
    /// </summary>
    /// <param name="fileName">The name of the file to be created.</param>
    /// <param name="content">The content of the file to be created.</param>
    /// <param name="useDefaultLayout">Whether to use the default code layout.</param>
    /// <returns>A new instance of <see cref="NinjadogContentFile"/>.</returns>
    protected NinjadogContentFile CreateNinjadogContentFile(
        string fileName, string content, bool useDefaultLayout = true)
    {
        return new NinjadogContentFile(fileName, content, Category, useDefaultLayout);
    }
}
