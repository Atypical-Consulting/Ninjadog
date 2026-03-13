namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Base class for auth templates that share the auth-enabled guard clause.
/// Subclasses implement <see cref="GenerateAuthContent"/> and optionally
/// override <see cref="ShouldGenerate"/> for additional guards.
/// </summary>
public abstract class AuthTemplateBase
    : NinjadogTemplate
{
    /// <inheritdoc />
    public sealed override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (!ShouldGenerate(auth))
        {
            return NinjadogContentFile.Empty;
        }

        return GenerateAuthContent(ninjadogSettings, auth!);
    }

    /// <summary>
    /// Determines whether this template should generate output for the given auth configuration.
    /// The default implementation returns true when auth is not null.
    /// Override to add additional guards (e.g., checking GenerateLoginEndpoint).
    /// </summary>
    /// <param name="auth">The auth configuration, or null when auth is disabled.</param>
    /// <returns>True if the template should generate output.</returns>
    protected virtual bool ShouldGenerate(NinjadogAuthConfiguration? auth)
    {
        return auth is not null;
    }

    /// <summary>
    /// Generates the auth-specific content. Only called when <see cref="ShouldGenerate"/> returns true.
    /// </summary>
    /// <param name="settings">The full Ninjadog settings.</param>
    /// <param name="auth">The non-null auth configuration.</param>
    /// <returns>The generated content file.</returns>
    protected abstract NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth);
}
