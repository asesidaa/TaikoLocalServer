using TaikoLocalServer.Domain.Enums;

namespace TaikoWebUI.Services;

/// <summary>
/// What the shell shows for the current page: the breadcrumb trail in the top bar, and the autosave status
/// for pages that save as you edit (null elsewhere).
/// Pages call <see cref="Set"/> when they initialise; the layout re-renders on <see cref="Changed"/>.
/// </summary>
public sealed class PageState
{
    public enum SaveStatus { Idle, Saving, Saved }

    /// <summary>One step of the trail; with <see cref="Href"/> it links back there (e.g. "Play Data" to its index).</summary>
    public sealed record Crumb(string Text, string? Href = null);

    public IReadOnlyList<Crumb> Crumbs { get; private set; } = [];

    public SaveStatus? Save { get; private set; }

    /// <summary>The server state in force, for the pill on admin pages; null until an admin page loads it.</summary>
    public ServerState? ServerState { get; private set; }

    public event Action? Changed;

    public void Set(params string[] crumbs) => Set(crumbs.Select(c => new Crumb(c)).ToArray());

    /// <summary>A Play Data page: "Play Data" (linking to the index) then the page's own steps.</summary>
    public void SetPlayData(string playData, long baid, params string[] rest) =>
        Set([new Crumb(playData, PlayDataPaths.Index(baid)), .. rest.Select(r => new Crumb(r))]);

    public void Set(params Crumb[] crumbs)
    {
        Crumbs = crumbs;
        Save = null;
        Changed?.Invoke();
    }

    public void SetServerState(ServerState state)
    {
        ServerState = state;
        Changed?.Invoke();
    }

    public void SetSave(SaveStatus status)
    {
        Save = status;
        Changed?.Invoke();
    }
}
