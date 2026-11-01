namespace TaikoWebUI.Services;

/// <summary>
/// A full-screen overlay rendered by the layout, outside every page column: a fixed overlay inside a sticky column is
/// trapped in that column's stacking context. One at a time; Escape or <see cref="Hide"/> closes it.
/// </summary>
public sealed class OverlayHost
{
    public RenderFragment? Content { get; private set; }

    public event Action? Changed;

    public void Show(RenderFragment content)
    {
        Content = content;
        Changed?.Invoke();
    }

    public void Hide()
    {
        if (Content is null) return;
        Content = null;
        Changed?.Invoke();
    }
}
