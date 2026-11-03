namespace TaikoLocalServer.Infrastructure.Extract;

/// <summary>Where a step is: <paramref name="Total"/> 0 until it knows its items; <paramref name="Item"/> the last one done.</summary>
public sealed record ReportProgress(string Step, int Done, int Total, string? Item);

/// <summary>Someone following a run (the server's Extract panel): told each step and item as it goes, and able to stop it.
/// Set on <see cref="Report.Watch"/> around the run; the command line runs without one.</summary>
public sealed record ReportWatch(Action<ReportProgress> Progress, CancellationToken Cancel);

/// <summary>What one step did. Printed as <c>&lt;label&gt;: N written, M already up to date, X missing, Y failed</c> and then
/// its <c>MISSING</c> and <c>FAILED</c> lines: the server's Extract panel reads these lines for progress.</summary>
public sealed class Report
{
    /// <summary>The run's watcher, if any; it flows into the threads <see cref="Each"/> starts.</summary>
    public static readonly AsyncLocal<ReportWatch?> Watch = new();

    public Report(string label)
    {
        Label = label;
        // A new step is where a stop between steps takes effect.
        Watch.Value?.Cancel.ThrowIfCancellationRequested();
        Watch.Value?.Progress(new ReportProgress(label, 0, 0, null));
    }

    public string Label { get; }
    public List<string> Written { get; } = [];
    public List<string> UpToDate { get; } = [];
    public List<string> Missing { get; } = [];
    public List<string> Failed { get; } = [];

    /// <summary>Converts <paramref name="items"/> on every core, then lists each in the items' order: up to date (skipped),
    /// written, or failed with the <see cref="InvalidDataException"/> <paramref name="convert"/> threw.</summary>
    public void Each<T>(IReadOnlyList<T> items, Func<T, string> name, Func<T, bool> upToDate, Action<T> convert)
    {
        var outcomes = new string?[items.Count]; // null: written; "": up to date; otherwise the failure
        var watch = Watch.Value;
        var done = 0;
        watch?.Progress(new ReportProgress(Label, 0, items.Count, null));
        try
        {
            Parallel.For(0, items.Count, new ParallelOptions { CancellationToken = watch?.Cancel ?? default }, i =>
            {
                if (upToDate(items[i]))
                    outcomes[i] = "";
                else
                {
                    try
                    {
                        convert(items[i]);
                    }
                    catch (InvalidDataException e)
                    {
                        outcomes[i] = $"{name(items[i])}: {e.Message}";
                    }
                }
                watch?.Progress(new ReportProgress(Label, Interlocked.Increment(ref done), items.Count, name(items[i])));
            });
        }
        catch (AggregateException e)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerExceptions[0]).Throw();
        }
        for (var i = 0; i < items.Count; i++)
        {
            if (outcomes[i] is null) Written.Add(name(items[i]));
            else if (outcomes[i] == "") UpToDate.Add(name(items[i]));
            else Failed.Add(outcomes[i]!);
        }
    }

    public void Print(TextWriter output)
    {
        output.WriteLine();
        output.WriteLine($"{Label}: {Written.Count} written, {UpToDate.Count} already up to date, {Missing.Count} missing, {Failed.Count} failed");
        foreach (var name in Missing)
            output.WriteLine($"  MISSING  {name}");
        foreach (var message in Failed)
            output.WriteLine($"  FAILED   {message}");
    }
}
