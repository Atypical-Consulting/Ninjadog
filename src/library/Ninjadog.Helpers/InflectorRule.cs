using System.Text.RegularExpressions;

namespace Ninjadog.Helpers;

internal sealed class InflectorRule(string pattern, string replacement)
{
    private readonly Regex _regex = new(pattern, RegexOptions.IgnoreCase);

    public string? Apply(string word)
    {
        return _regex.IsMatch(word)
            ? _regex.Replace(word, replacement)
            : null;
    }
}
