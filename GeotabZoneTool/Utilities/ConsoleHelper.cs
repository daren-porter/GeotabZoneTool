namespace GeotabZoneTool.Utilities;

public static class ConsoleHelper
{
    public static void WriteLine(string value) => WriteLineImpl(value);

    public static void WriteLine() => WriteLineImpl();

    public static void MarkupLineInterpolated(FormattableString value) => AnsiConsole.MarkupLineInterpolated(value);

    public static void ToolStartHeader()
    {
        AnsiConsole.Clear();
        WriteImpl(new FigletText("Geotab Zone Tool").LeftAligned().Color(Color.SlateBlue3_1));
        WriteLine();
    }

    public static void ReportResults(ZoneResults results, int invalidZoneCount, string outputPath, Stopwatch stopwatch)
    {
        var overlapCount = results.ZonesWithOverlaps?.Count;
        MarkupLineInterpolated($"\n[blue]{overlapCount}[/] zones found that have at least one other zone overlapping.");
        
        if (invalidZoneCount > 0)
            MarkupLineInterpolated($"[yellow]{invalidZoneCount}[/] zones found with invalid coordinates.");

        WriteLineImpl();
        WriteLineImpl($"Results will be written in JSON to the following location:\n{outputPath}\n");
        WriteLineImpl($"Process ran for {stopwatch.Elapsed}");
    }

    public static async Task StartProgressTaskAsync(Func<ProgressContext, Task> task)
    {
        await AnsiConsole.Progress()
                         .Columns(new TaskDescriptionColumn { Alignment = Justify.Left },
                                  new ProgressBarColumn(),
                                  new PercentageColumn(),
                                  new ElapsedTimeColumn(),
                                  new SpinnerColumn())
                         .StartAsync(task);
    }

    public static void ReadKeyWithText(string? value = null)
    {
        if (value is not null)
            WriteLineImpl(value);
        
        WriteLineImpl("Press any key to exit...");
        Console.ReadKey();
    }

    private static void WriteImpl(IRenderable value) => AnsiConsole.Write(value);
    private static void WriteLineImpl(string value) => AnsiConsole.WriteLine(value);
    private static void WriteLineImpl() => AnsiConsole.WriteLine();
}