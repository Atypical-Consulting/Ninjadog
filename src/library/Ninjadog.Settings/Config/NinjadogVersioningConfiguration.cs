namespace Ninjadog.Settings.Config;

/// <summary>
/// Represents the API versioning configuration for the generated API.
/// </summary>
/// <param name="Strategy">The versioning strategy: "UrlPath" for route-based (/v1/resource) or "HeaderBased" for header-based (X-Api-Version: 1). Default is "UrlPath".</param>
/// <param name="DefaultVersion">The default API version number. Default is 1.</param>
/// <param name="Prefix">The version prefix for URL path strategy (e.g., "v" produces /v1/). Default is "v".</param>
/// <param name="HeaderName">The header name for header-based strategy. Default is "X-Api-Version".</param>
public record NinjadogVersioningConfiguration(
    string Strategy = NinjadogVersioningConfiguration.UrlPathStrategy,
    int DefaultVersion = 1,
    string Prefix = "v",
    string HeaderName = "X-Api-Version")
{
    /// <summary>URL path versioning strategy (e.g., /v1/resource).</summary>
    public const string UrlPathStrategy = "UrlPath";

    /// <summary>Header-based versioning strategy (e.g., X-Api-Version: 1).</summary>
    public const string HeaderBasedStrategy = "HeaderBased";

    /// <summary>Gets a value indicating whether this configuration uses URL path versioning.</summary>
    public bool IsUrlPath => Strategy.Equals(UrlPathStrategy, StringComparison.OrdinalIgnoreCase);
}
