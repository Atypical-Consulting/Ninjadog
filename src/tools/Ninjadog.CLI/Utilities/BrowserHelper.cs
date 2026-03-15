using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ninjadog.CLI.Utilities;

/// <summary>
/// Opens a URL in the default browser across platforms.
/// </summary>
internal static class BrowserHelper
{
    /// <summary>
    /// Opens the specified URL in the default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current OS is not recognized.</exception>
    public static void OpenBrowser(string url)
    {
        var (fileName, arguments) = true switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => ("open", url),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => ("cmd", $"/c start \"{url}\""),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => ("xdg-open", url),
            _ => throw new PlatformNotSupportedException("Cannot open browser on this platform.")
        };

        using var process = Process.Start(new ProcessStartInfo(fileName, arguments) { UseShellExecute = false });
    }
}
