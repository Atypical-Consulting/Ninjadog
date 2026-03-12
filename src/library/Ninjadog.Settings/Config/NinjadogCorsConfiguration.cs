namespace Ninjadog.Settings.Config;

/// <summary>
/// Represents the CORS configuration for the generated API.
/// </summary>
/// <param name="Origins">The allowed origins for CORS requests.</param>
/// <param name="Methods">The allowed HTTP methods for CORS requests.</param>
/// <param name="Headers">The allowed headers for CORS requests.</param>
public record NinjadogCorsConfiguration(
    string[] Origins,
    string[]? Methods = null,
    string[]? Headers = null);
