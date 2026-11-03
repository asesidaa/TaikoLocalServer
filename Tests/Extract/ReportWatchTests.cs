using TaikoLocalServer.Infrastructure.Extract;

namespace TaikoLocalServer.Tests.Extract;

// The Extract panel's progress and Stop: a step tells its watcher each item as it goes, and a stop ends the run there.
public sealed class ReportWatchTests
{
    [Fact]
    public void AStep_ReportsEachItem_UpToItsTotal()
    {
        var seen = new List<ReportProgress>();
        Report.Watch.Value = new ReportWatch(p => { lock (seen) seen.Add(p); }, CancellationToken.None);
        try
        {
            var report = new Report("models");
            report.Each([1, 2, 3, 4, 5], i => $"item{i}", _ => false, _ => { });
            Assert.Equal(new ReportProgress("models", 0, 0, null), seen[0]);
            Assert.Equal(5, seen.Max(p => p.Done));
            Assert.All(seen.Where(p => p.Done > 0), p => Assert.Equal(5, p.Total));
            Assert.Equal(5, report.Written.Count);
        }
        finally
        {
            Report.Watch.Value = null;
        }
    }

    [Fact]
    public void AStop_EndsTheStepAndTheRun()
    {
        using var stop = new CancellationTokenSource();
        var converted = 0;
        Report.Watch.Value = new ReportWatch(_ => { }, stop.Token);
        try
        {
            var report = new Report("models");
            Assert.ThrowsAny<OperationCanceledException>(() =>
                report.Each(Enumerable.Range(0, 10_000).ToList(), i => $"item{i}", _ => false, _ =>
                {
                    if (Interlocked.Increment(ref converted) == 10) stop.Cancel();
                }));
            Assert.True(converted < 10_000, "items after the stop aren't converted");
            // The next step doesn't start.
            Assert.ThrowsAny<OperationCanceledException>(() => new Report("tones"));
        }
        finally
        {
            Report.Watch.Value = null;
        }
    }
}
