namespace TaikoWebUI.Services;

/// <summary>Short confirmation messages, shown bottom-right by the layout for about two seconds.</summary>
public sealed class ToastService
{
    public string? Message { get; private set; }
    public bool IsError { get; private set; }

    public event Action? Changed;

    private CancellationTokenSource? hide;

    public void Show(string message, bool isError = false)
    {
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
