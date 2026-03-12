namespace Ninjadog.Settings.Config;

/// <summary>
/// Represents the JWT authentication configuration for the generated API.
/// </summary>
/// <param name="Provider">The authentication provider type. Currently only "jwt" is supported.</param>
/// <param name="Issuer">The token issuer URL.</param>
/// <param name="Audience">The token audience identifier.</param>
/// <param name="TokenExpirationMinutes">The token expiration time in minutes.</param>
/// <param name="Roles">Optional array of role names for authorization policies.</param>
/// <param name="GenerateLoginEndpoint">Whether to generate a login endpoint.</param>
/// <param name="GenerateRegisterEndpoint">Whether to generate a register endpoint.</param>
public record NinjadogAuthConfiguration(
    string Provider = "jwt",
    string Issuer = "https://localhost",
    string Audience = "api",
    int TokenExpirationMinutes = 60,
    string[]? Roles = null,
    bool GenerateLoginEndpoint = true,
    bool GenerateRegisterEndpoint = true);
