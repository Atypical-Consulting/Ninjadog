using System.Diagnostics.CodeAnalysis;

namespace Ninjadog.Settings.Extensions.Entities.Properties;

/// <summary>
/// Represents a property of a Ninjadog entity.
/// </summary>
/// <param name="IsKey">Indicates whether the property is a key field. Defaults to false.</param>
/// <typeparam name="T">The data type of the property.</typeparam>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1649:File name should match first type name",
    Justification = "This is a generic type.")]
public record NinjadogEntityProperty<T>(
    bool IsKey = false)
    : NinjadogEntityProperty(typeof(T).Name, IsKey)
    where T : notnull;
