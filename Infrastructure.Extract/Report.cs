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
