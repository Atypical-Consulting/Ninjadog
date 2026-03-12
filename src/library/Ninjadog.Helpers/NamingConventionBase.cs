namespace Ninjadog.Helpers;

/// <summary>
/// Serves as the base record for handling naming conventions.
/// This record provides a common structure for generating various naming formats based on a given Pascal case string.
/// </summary>
/// <param name="Pascal">The Pascal case string from which other naming formats are derived.</param>
public abstract record NamingConventionBase(string Pascal)
{
    /// <summary>
    /// Gets the camel case version of the Pascal case string.
    /// </summary>
    protected virtual string Camel
        => Pascal.Camelize();

    /// <summary>
    /// Gets the dashed (kebab case) version of the Pascal case string.
    /// </summary>
    protected virtual string Dashed
        => Pascal.Underscore().Dasherize();

    /// <summary>
    /// Gets the humanized (lowercase with spaces) version of the Pascal case string.
    /// </summary>
    protected virtual string Humanized
        => Pascal.Underscore().Humanize().ToLowerInvariant();

    /// <summary>
    /// Gets the pluralized camel case version of the Pascal case string.
    /// </summary>
    protected virtual string CamelPlural
        => Camel.Pluralize();

    /// <summary>
    /// Gets the pluralized Pascal case version of the original string.
    /// </summary>
    protected virtual string PascalPlural
        => Pascal.Pluralize();

    /// <summary>
    /// Gets the pluralized dashed (kebab case) version of the Pascal case string.
    /// </summary>
    protected virtual string DashedPlural
        => PascalPlural.Underscore().Dasherize();

    /// <summary>
    /// Gets the pluralized humanized (lowercase with spaces) version of the Pascal case string.
    /// </summary>
    protected virtual string HumanizedPlural
        => PascalPlural.Underscore().Humanize().ToLowerInvariant();
}
