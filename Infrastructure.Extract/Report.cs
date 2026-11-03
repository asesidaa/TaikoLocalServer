namespace TaikoLocalServer.Infrastructure.Extract;

/// <summary>What one step did. Printed as <c>&lt;label&gt;: N written, M already up to date, X missing, Y failed</c> and then
/// its <c>MISSING</c> and <c>FAILED</c> lines: the server's Extract panel reads these lines for progress.</summary>
public sealed class Report(string label)
{
    public string Label { get; } = label;
    public List<string> Written { get; } = [];
    public List<string> UpToDate { get; } = [];
    public List<string> Missing { get; } = [];
    public List<string> Failed { get; } = [];

    /// <summary>Converts <paramref name="items"/> on every core, then lists each in the items' order: up to date (skipped),
    /// written, or failed with the <see cref="InvalidDataException"/> <paramref name="convert"/> threw.</summary>
    public void Each<T>(IReadOnlyList<T> items, Func<T, string> name, Func<T, bool> upToDate, Action<T> convert)
    {
        var outcomes = new string?[items.Count]; // null: written; "": up to date; otherwise the failure
        try
        {
            Parallel.For(0, items.Count, i =>
            {
                if (upToDate(items[i]))
                {
                    outcomes[i] = "";
                    return;
                }
                try
                {
                    convert(items[i]);
                }
                catch (InvalidDataException e)
                {
                    outcomes[i] = $"{name(items[i])}: {e.Message}";
                }
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
