using Ninjadog.Helpers;

namespace Ninjadog.Tests.Helpers;

public class IndentedStringBuilderTests
{
    [Fact]
    public void Append_WithNoIndent_AppendsDirectly()
    {
        var sb = new IndentedStringBuilder(0);
        sb.Append("hello");
        Assert.Equal("hello", sb.ToString());
    }

    [Fact]
    public void Append_WithIndent_PrependsSpaces()
    {
        var sb = new IndentedStringBuilder(1);
        sb.AppendLine("hello").Append("world");
        var result = sb.ToString();
        Assert.Contains("    hello", result);
        Assert.Contains("    world", result);
    }

    [Fact]
    public void AppendChar_AppendsCharacter()
    {
        var sb = new IndentedStringBuilder(0);
        sb.Append('x');
        Assert.Equal("x", sb.ToString());
    }

    [Fact]
    public void AppendStrings_AppendsAllStrings()
    {
        var sb = new IndentedStringBuilder(0);
        sb.Append(["hello", " ", "world"]);
        Assert.Equal("hello world", sb.ToString());
    }

    [Fact]
    public void AppendChars_AppendsAllChars()
    {
        var sb = new IndentedStringBuilder(0);
        sb.Append(['a', 'b', 'c']);
        Assert.Equal("abc", sb.ToString());
    }

    [Fact]
    public void AppendLine_EmptyString_AddsNewLine()
    {
        var sb = new IndentedStringBuilder(0);
        sb.AppendLine();
        Assert.Equal(Environment.NewLine, sb.ToString());
    }

    [Fact]
    public void AppendLine_WithValue_AddsValueAndNewLine()
    {
        var sb = new IndentedStringBuilder(0);
        sb.AppendLine("hello");
        Assert.Equal($"hello{Environment.NewLine}", sb.ToString());
    }

    [Fact]
    public void AppendLines_SplitsAndIndentsEachLine()
    {
        var sb = new IndentedStringBuilder(1);
        sb.AppendLines($"line1{Environment.NewLine}line2");
        var result = sb.ToString();
        Assert.Contains("    line1", result);
        Assert.Contains("    line2", result);
    }

    [Fact]
    public void AppendLines_SkipFinalNewline_OmitsTrailingNewline()
    {
        var sb = new IndentedStringBuilder(0);
        sb.AppendLines("hello", skipFinalNewline: true);
        Assert.Equal("hello", sb.ToString());
    }

    [Fact]
    public void Length_ReturnsCurrentLength()
    {
        var sb = new IndentedStringBuilder(0);
        sb.Append("hello");
        Assert.Equal(5, sb.Length);
    }

    [Fact]
    public void Clear_ResetsBuilder()
    {
        var sb = new IndentedStringBuilder(2).Append("hello").Clear();
        Assert.Equal(0, sb.Length);
        Assert.Equal(string.Empty, sb.ToString());
    }

    [Fact]
    public void IncrementIndent_IncreasesIndentation()
    {
        var sb = new IndentedStringBuilder(0);
        sb.IncrementIndent().AppendLine("hello").IncrementIndent().Append("world");
        var result = sb.ToString();
        Assert.Contains("    hello", result);
        Assert.Contains("        world", result);
    }

    [Fact]
    public void IncrementIndent_ByCount_IncreasesMultipleLevels()
    {
        var sb = new IndentedStringBuilder(0);
        sb.IncrementIndent(2).Append("hello");
        Assert.Contains("        hello", sb.ToString());
    }

    [Fact]
    public void DecrementIndent_DecreasesIndentation()
    {
        var sb = new IndentedStringBuilder(0);
        sb.IncrementIndent().IncrementIndent().DecrementIndent().Append("hello");
        Assert.Equal("    hello", sb.ToString());
    }

    [Fact]
    public void DecrementIndent_DoesNotGoBelowZero()
    {
        var sb = new IndentedStringBuilder(0);
        sb.DecrementIndent().Append("hello");
        Assert.Equal("hello", sb.ToString());
    }

    [Fact]
    public void DecrementIndent_ByCount_DoesNotThrow()
    {
        var sb = new IndentedStringBuilder(1);
        sb.DecrementIndent(5).Append("hello");
        Assert.Contains("hello", sb.ToString());
    }

    [Fact]
    public void Indent_ScopedDisposable_IncrementsAndDecrements()
    {
        var sb = new IndentedStringBuilder(0);
        sb.AppendLine("outer");

        using (sb.Indent())
        {
            sb.AppendLine("inner");
        }

        sb.Append("outer again");
        var result = sb.ToString();
        Assert.Contains("outer", result);
        Assert.Contains("    inner", result);
        Assert.DoesNotContain("    outer again", result);
    }

    [Fact]
    public void SuspendIndent_TemporarilyDisablesIndentation()
    {
        var sb = new IndentedStringBuilder(2);
        sb.AppendLine("indented");

        using (sb.SuspendIndent())
        {
            sb.AppendLine("not indented");
        }

        sb.Append("indented again");
        var result = sb.ToString();
        Assert.StartsWith("        indented", result);
        Assert.Contains($"{Environment.NewLine}not indented", result);
        Assert.Contains("        indented again", result);
    }

    [Fact]
    public void FluentChaining_WorksCorrectly()
    {
        var result = new IndentedStringBuilder(0)
            .Append("hello")
            .Append(' ')
            .Append("world")
            .ToString();

        Assert.Equal("hello world", result);
    }
}
