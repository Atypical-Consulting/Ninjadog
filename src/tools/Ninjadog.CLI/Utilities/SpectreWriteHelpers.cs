namespace Ninjadog.CLI.Utilities;

internal static class SpectreWriteHelpers
{
    public static void WriteNinjadog()
    {
        Write(
            new FigletText("Ninjadog")
                .LeftJustified()
                .Color(Color.Red));
        MarkupLine("[bold]by Atypical Consulting SRL[/]");
        WriteLine();
    }

    public static void WriteSettingsTable(Action<Table> action)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(string.Empty);
        table.AddColumn(string.Empty);
        table.Columns[0].Width(24);
        table.Columns[1].Width(48);
        table.HideHeaders();
        action(table);

        Write(table);
    }

    public static void WriteTextPath(string path)
    {
        var textPath = new TextPath(path);
        textPath.RootStyle(new Style(foreground: Color.Red));
        textPath.SeparatorStyle(new Style(foreground: Color.Yellow));
        textPath.StemStyle(new Style(foreground: Color.Blue));
        textPath.LeafStyle(new Style(foreground: Color.Aqua));

        Write(textPath);
    }
}
