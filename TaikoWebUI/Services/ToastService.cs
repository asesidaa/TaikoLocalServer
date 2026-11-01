namespace TaikoWebUI.Services;

/// <summary>Short confirmation messages, shown bottom-right by the layout for about two seconds.</summary>
public sealed class ToastService
{
    public string? Message { get; private set; }
    public bool IsError { get; private set; }

    public event Action? Changed;

    private CancellationTokenSource? hide;
    private DateTime keepUntil;

    /// <summary>Shows a message that other toasts don't replace for a moment (e.g. the page's own "Save failed" after it).</summary>
    public void ShowFirm(string message)
    {
        keepUntil = default;
        Show(message);
        keepUntil = DateTime.UtcNow.AddSeconds(2);
    }

    public void Show(string message, bool isError = false)
    {
        if (DateTime.UtcNow < keepUntil) return;
        Message = message;
        IsError = isError;
        Changed?.Invoke();
        hide?.Cancel();
        hide = new CancellationTokenSource();
        _ = HideLater(hide.Token);
    }

    private async Task HideLater(CancellationToken token)
    {
        try { await Task.Delay(IsError ? TimeSpan.FromSeconds(4) : TimeSpan.FromSeconds(2), token); }
        catch (TaskCanceledException) { return; }
        Message = null;
        Changed?.Invoke();
    }
}
