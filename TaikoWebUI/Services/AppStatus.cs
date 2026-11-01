using System.Net.Http.Json;

namespace TaikoWebUI.Services;

/// <summary>
/// What the shell shows about the app itself (the System states): the server state (banners, the players' offline
/// page), the connection (the bar, with retries), and a signed-out session (the sign-in dialog). Created before the
/// HttpClient, since <see cref="AppStatusHandler"/> reports to it; one per tab.
/// </summary>
public sealed class AppStatus
{
    private static readonly TimeSpan[] Backoff = [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(16), TimeSpan.FromSeconds(30)];
    private static readonly TimeSpan StateRefresh = TimeSpan.FromSeconds(60);

    private HttpClient? client;
    private DateTime stateReadAt;
    private TaskCompletionSource online = Done();
    private CancellationTokenSource? wait;
    private TaskCompletionSource<string?>? signIn;
    private int heldSaves;

    public event Action? Changed;
    /// <summary>A save was refused because of maintenance (the shell shows a toast).</summary>
    public event Action? SavingPaused;

    /// <summary>The server state as last read (api/ServerState/Current); null until read.</summary>
    public ServerStateSettings? Server { get; private set; }

    public bool Offline { get; private set; }
    /// <summary>Seconds until the next try while <see cref="Offline"/>.</summary>
    public int RetryIn { get; private set; }
    /// <summary>For a few seconds after the connection came back; true if saves were waiting for it.</summary>
    public bool? BackOnline { get; private set; }

    /// <summary>A request was refused as signed out: the shell asks for the password again.</summary>
    public bool SignInNeeded => signIn is not null;

    public void Start(HttpClient http, ServerStateSettings? server)
    {
        client = http;
        Server = server;
        stateReadAt = DateTime.UtcNow;
        _ = KeepStateFresh();
    }

    // ---- Server state ----

    /// <summary>Reads the state again if it's older than <paramref name="maxAge"/> (each navigation, any 503).</summary>
    public async Task RefreshState(TimeSpan maxAge)
    {
        if (client is null || DateTime.UtcNow - stateReadAt < maxAge) return;
        stateReadAt = DateTime.UtcNow;
        try
        {
            Server = await client.GetFromJsonAsync<ServerStateSettings>("api/ServerState/Current");
            Changed?.Invoke();
        }
        catch (HttpRequestException)
        {
            // The connection bar covers it.
        }
    }

    private async Task KeepStateFresh()
    {
        while (true)
        {
            await Task.Delay(StateRefresh);
            await RefreshState(StateRefresh - TimeSpan.FromSeconds(1));
        }
    }

    internal void Refused(ServerState state, bool save)
    {
        _ = RefreshState(TimeSpan.Zero);
        if (save && state == ServerState.Maintenance) SavingPaused?.Invoke();
    }

    // ---- Connection ----

    /// <summary>Completes when the connection is back (right away while online).</summary>
    internal Task WhenOnline(CancellationToken cancellationToken) => online.Task.WaitAsync(cancellationToken);

    internal void SaveHeld() => Interlocked.Increment(ref heldSaves);

    internal void Lost()
    {
        if (Offline) return;
        Offline = true;
        BackOnline = null;
        online = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Changed?.Invoke();
        _ = Reconnect();
    }

    internal void Reached()
    {
        if (!Offline) return;
        Offline = false;
        BackOnline = Interlocked.Exchange(ref heldSaves, 0) > 0;
        online.TrySetResult();
        Changed?.Invoke();
        _ = HideBackOnline();
    }

    /// <summary>"Retry now".</summary>
    public void RetryNow() => wait?.Cancel();

    private async Task Reconnect()
    {
        for (var attempt = 0; Offline; attempt++)
        {
            wait = new CancellationTokenSource();
            for (RetryIn = (int)Backoff[Math.Min(attempt, Backoff.Length - 1)].TotalSeconds; RetryIn > 0 && !wait.IsCancellationRequested; RetryIn--)
            {
                Changed?.Invoke();
                try { await Task.Delay(1000, wait.Token); } catch (TaskCanceledException) { }
            }
            RetryIn = 0;
            Changed?.Invoke();
            // Any answer means the server is reachable (the handler then calls Reached).
            stateReadAt = DateTime.MinValue;
            await RefreshState(TimeSpan.Zero);
        }
    }

    private async Task HideBackOnline()
    {
        await Task.Delay(3000);
        if (Offline) return;
        BackOnline = null;
        Changed?.Invoke();
    }

    // ---- Signed out ----

    /// <summary>Waits for the person to sign in again: the new token, or null if they chose another account.</summary>
    internal Task<string?> SignInAgain()
    {
        if (signIn is null)
        {
            signIn = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
            Changed?.Invoke();
        }
        return signIn.Task;
    }

    /// <summary>The token from the last time the person signed in again.</summary>
    internal string? LatestToken { get; private set; }

    /// <summary>The dialog's outcome: the new token, or null (a different account, via the login page).</summary>
    public void SignedInAgain(string? token)
    {
        LatestToken = token;
        var pending = signIn;
        signIn = null;
        Changed?.Invoke();
        pending?.TrySetResult(token);
    }

    private static TaskCompletionSource Done()
    {
        var done = new TaskCompletionSource();
        done.SetResult();
        return done;
    }
}
