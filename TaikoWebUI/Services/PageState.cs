namespace TaikoWebUI.Services;

/// <summary>
/// What the shell shows for the current page: the breadcrumb trail in the top bar, and the autosave status
/// for pages that save as you edit (null elsewhere).
/// Pages call <see cref="Set"/> when they initialise; the layout re-renders on <see cref="Changed"/>.
/// </summary>
public sealed class PageState
{
    public enum SaveStatus { Idle, Saving, Saved }

    public IReadOnlyList<string> Crumbs { get; private set; } = [];

    public SaveStatus? Save { get; private set; }

    public event Action? Changed;

    public void Set(params string[] crumbs)
    {
        Crumbs = crumbs;
        Save = null;
        Changed?.Invoke();
    }

    public void SetSave(SaveStatus status)
    {
        Save = status;
        Changed?.Invoke();
    }
}
