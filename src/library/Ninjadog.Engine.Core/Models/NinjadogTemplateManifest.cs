namespace Ninjadog.Engine.Core.Models;

/// <summary>
/// Represents an abstract base for a Ninjadog template manifest.
/// This class serves as a blueprint for defining the metadata and associated files for a specific template.
/// </summary>
public abstract class NinjadogTemplateManifest
{
    private const string DefaultName = "Ninjadog Template";
    private const string DefaultDescription = "A Ninjadog template.";
    private const string DefaultVersion = "1.0.0";
    private const string DefaultAuthor = "Atypical Consulting SRL";
    private const string DefaultLicense = "Proprietary";

    /// <summary>
    /// Gets the name of the template manifest.
    /// </summary>
    public virtual string Name { get; init; } = DefaultName;

    /// <summary>
    /// Gets the description of the template manifest.
    /// </summary>
    public virtual string Description { get; init; } = DefaultDescription;

    /// <summary>
    /// Gets the version of the template manifest.
    /// </summary>
    public virtual string Version { get; init; } = DefaultVersion;

    /// <summary>
    /// Gets the author of the template manifest.
    /// </summary>
    public virtual string Author { get; init; } = DefaultAuthor;

    /// <summary>
    /// Gets the license under which the template manifest is distributed.
    /// </summary>
    public virtual string License { get; init; } = DefaultLicense;

    /// <summary>
    /// Gets the collection of NuGet packages required by this template.
    /// </summary>
    public virtual List<string> NuGetPackages { get; init; } = [];

    /// <summary>
    /// Gets the collection of template files associated with this manifest.
    /// </summary>
    public virtual NinjadogTemplates Templates { get; init; } = [];
}
