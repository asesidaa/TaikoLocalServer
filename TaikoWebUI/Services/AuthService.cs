using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using TaikoLocalServer.Contracts.AdminApi.Authorization;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoWebUI.Services;

/// <summary>
/// Thin facade in front of <see cref="JwtAuthenticationStateProvider"/> + the server-supplied
/// <see cref="ClientAuthConfigResponse"/>. Properties cache the latest known auth state and the
/// legacy <see cref="LoginStatusChanged"/> event keeps existing call sites compiling — the long-term
/// plan is for pages to switch to <c>&lt;AuthorizeView&gt;</c> + cascading <c>AuthenticationState</c>
/// and shrink this class down to its HTTP wrappers.
/// </summary>
public sealed class AuthService : IDisposable
{
    private readonly HttpClient client;
    private readonly JwtAuthenticationStateProvider authStateProvider;
    private readonly ClientAuthConfigResponse authConfig;

    public event EventHandler? LoginStatusChanged;

    /// <summary>Local (no accounts or login) or Server.</summary>
    public ServerMode Mode => authConfig.Mode;
    public bool LoginRequired => authConfig.AuthenticationRequired;
    /// <summary>Server mode with no admin yet: Register takes the one-time setup code from the server console.</summary>
    public bool SetupPending => authConfig.SetupPending;
    public bool OnlyAdmin => authConfig.OnlyAdmin;
    public int BoundAccessCodeUpperLimit => authConfig.BoundAccessCodeUpperLimit;
    public SignUpMode SignUpMode => authConfig.SignUpMode;
    public bool AllowUserDelete => authConfig.AllowUserDelete;
    public bool AllowFreeProfileEditing => authConfig.AllowFreeProfileEditing;

    /// <summary>
    /// True when the user holds a valid server-issued bearer token. Stays false in local
    /// mode (matching the existing UX where login UI is hidden when authentication is off)
    /// even though the synthetic principal in the state provider claims the Admin role.
    /// </summary>
    public bool IsLoggedIn { get; private set; }

    /// <summary>
    /// True when the bearer-token-bearing user has the Admin role. Stays false in local mode
    /// so existing call sites keep using the <c>IsAdmin || !LoginRequired</c> idiom; the
    /// state provider's synthetic admin handles the new <c>AuthorizeView</c> consumers.
    /// </summary>
    public bool IsAdmin { get; private set; }

    public TwoFactorMode TwoFactor => authConfig.TwoFactor;

    /// <summary>The server draws avatars (as of this page load), so the player card doesn't make and upload one.</summary>
    public bool ServerAvatars => authConfig.ServerAvatars;
    /// <summary>Player cards show still images the server draws (Player Settings › Enable 3D Renderer off).</summary>
    public bool ServerStills => authConfig.ServerStills;

    /// <summary>An admin whose server requires 2-step: they're a player until they turn it on in Settings › Security.</summary>
    public bool TwoFactorSetupRequired { get; private set; }

    /// <summary>After <see cref="Login"/> returned 8: the card's last 4 digits, for the verify step.</summary>
    public string PendingLast4 { get; private set; } = "";

    private string pendingToken = "";
    private readonly ILocalStorageService localStorage;

    // "Trust this browser": one per browser; the server ignores it for any other account.
    private const string TrustKey = "auth.trustedBrowser";
    // The card this browser signed in with, so a signed-out session can sign in again with just the password.
    private const string CardKey = "auth.card";

    private uint loggedInBaid;
    private string? sessionId;

    public AuthService(
        HttpClient client,
        JwtAuthenticationStateProvider authStateProvider,
        ClientAuthConfigResponse authConfig,
        ILocalStorageService localStorage)
    {
        this.localStorage = localStorage;
        this.client = client;
        this.authStateProvider = authStateProvider;
        this.authConfig = authConfig;

        authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        // Eagerly read the initial state so synchronous property accesses on first render
        // (NavMenu/UserCard/pages all read AuthService.IsLoggedIn before any render hook fires)
        // observe correct values rather than the default false/false.
        _ = RefreshFromStateAsync(authStateProvider.GetAuthenticationStateAsync());
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _ = RefreshFromStateAsync(task);
    }

    private async Task RefreshFromStateAsync(Task<AuthenticationState> stateTask)
    {
        var state = await stateTask;
        UpdateFromPrincipal(state.User);
        LoginStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateFromPrincipal(ClaimsPrincipal user)
    {
        if (!authConfig.AuthenticationRequired)
        {
            // Preserve the legacy "no login UI in local mode" semantics: the synthetic admin
            // principal exists for AuthorizeView consumers, but AuthService's facade still
            // reports IsLoggedIn=false so NavMenu's login/logout chrome stays hidden.
            IsLoggedIn = false;
            IsAdmin = false;
            loggedInBaid = 0;
            return;
        }

        var isAuthenticated = user.Identity?.IsAuthenticated == true;
        IsLoggedIn = isAuthenticated;
        IsAdmin = isAuthenticated && user.IsAdmin();
        TwoFactorSetupRequired = isAuthenticated && user.FindFirst("tfa_setup") is not null;
        sessionId = isAuthenticated ? user.FindFirst("sid")?.Value : null;
        loggedInBaid = isAuthenticated ? user.GetBaid() ?? 0u : 0u;
    }

    public async Task<int> Login(string inputAccessCode, string inputPassword)
    {
        var request = new LoginRequest
        {
            AccessCode = NormalizeAccessCode(inputAccessCode),
            Password = inputPassword,
            TrustToken = await localStorage.GetItemAsStringAsync(TrustKey),
        };

        var responseMessage = await client.PostAsJsonAsync("api/Auth/Login", request);
        if (responseMessage.IsSuccessStatusCode) await localStorage.SetItemAsStringAsync(CardKey, request.AccessCode);

        if (!responseMessage.IsSuccessStatusCode)
        {
            return MapErrorMessage(await responseMessage.Content.ReadAsStringAsync(), msg => msg switch
            {
                "Invalid Credentials" => 2,
                "User Not Registered" => 4,
                "Account Locked" => 6,
                "Card Banned" => 7,
                _ => 5
            }, fallback: 5);
        }

        using var json = JsonDocument.Parse(await responseMessage.Content.ReadAsStringAsync());
        var root = json.RootElement;
        if (root.TryGetProperty("twoFactor", out var tf) && tf.ValueKind == JsonValueKind.True)
        {
            // Password accepted; a code from the authenticator app comes next (VerifyTwoFactor).
            pendingToken = root.GetProperty("pendingToken").GetString() ?? "";
            PendingLast4 = root.GetProperty("last4").GetString() ?? "";
            return 8;
        }
        if (!root.TryGetProperty("authToken", out var token) || token.GetString() is not { } authToken) return 5;

        await authStateProvider.MarkUserAsAuthenticatedAsync(authToken);
        return 1;
    }

    /// <summary>Login step 2. 1 signed in, 9 wrong code, 10 step 1 expired (start over), 6 locked, 5 other.</summary>
    public async Task<int> VerifyTwoFactor(string code, bool trustBrowser)
    {
        var response = await client.PostAsJsonAsync("api/Auth/Verify2fa", new VerifyTwoFactorRequest { PendingToken = pendingToken, Code = code, TrustBrowser = trustBrowser });
        if (!response.IsSuccessStatusCode)
        {
            return MapErrorMessage(await response.Content.ReadAsStringAsync(), msg => msg switch
            {
                "Wrong Code" => 9,
                "Sign In Expired" => 10,
                "Account Locked" => 6,
                _ => 5
            }, fallback: 5);
        }
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (json.RootElement.GetProperty("authToken").GetString() is not { } authToken) return 5;
        if (json.RootElement.TryGetProperty("trustToken", out var trust)) await localStorage.SetItemAsStringAsync(TrustKey, trust.GetString() ?? "");
        pendingToken = "";
        await authStateProvider.MarkUserAsAuthenticatedAsync(authToken);
        return 1;
    }

    /// <summary>The card the session signed in with, for signing in again (the System states' dialog); null if unknown.</summary>
    public async Task<string?> SignedInCard() => await localStorage.GetItemAsStringAsync(CardKey) is { Length: > 0 } card ? card : null;

    /// <summary>The token the API calls now carry.</summary>
    public string? CurrentToken => client.DefaultRequestHeaders.Authorization?.Parameter;

    /// <summary>A new token from the server (turning on 2-step unlocked admin access).</summary>
    public Task UseToken(string authToken) => authStateProvider.MarkUserAsAuthenticatedAsync(authToken);

    public async Task<int> Register(string inputCardNum, DateTime inputDateTime, string inputPassword,
        string inputConfirmPassword, string inviteCode, string setupCode = "")
    {
        if (OnlyAdmin && setupCode.Length == 0) return 0;

        if (inputPassword != inputConfirmPassword) return 2;

        var request = new RegisterRequest
        {
            AccessCode = NormalizeAccessCode(inputCardNum),
            Password = inputPassword,
            LastPlayDateTime = inputDateTime,
            InviteCode = inviteCode,
            SetupCode = setupCode
        };

        var responseMessage = await client.PostAsJsonAsync("api/Auth/Register", request);
        if (responseMessage.IsSuccessStatusCode) return 1;

        return MapErrorMessage(await responseMessage.Content.ReadAsStringAsync(), msg => msg switch
        {
            "Access Code Not Found" => 3,
            "User Already Registered" => 4,
            "Wrong Last Play Time" => 5,
            "Wrong Setup Code" => 7,
            "Invite Code Required" => 8,
            "Sign-up Closed" => 9,
            _ => 6
        }, fallback: 6);
    }

    public async Task<int> ChangePassword(string inputOldPassword, string inputNewPassword,
        string inputConfirmNewPassword)
    {
        if (OnlyAdmin) return 0;

        if (inputNewPassword != inputConfirmNewPassword) return 2;

        var request = new ChangePasswordRequest
        {
            OldPassword = inputOldPassword,
            NewPassword = inputNewPassword
        };

        var responseMessage = await client.PostAsJsonAsync("api/Auth/ChangePassword", request);
        if (responseMessage.IsSuccessStatusCode) return 1;

        return MapErrorMessage(await responseMessage.Content.ReadAsStringAsync(), msg => msg switch
        {
            "User Not Registered" => 5,
            "Wrong Old Password" => 4,
            _ => 6
        }, fallback: 6);
    }

    /// <summary>Ends this session on the server too, so the token stops working even if it was copied.</summary>
    public async Task Logout()
    {
        if (sessionId is { } sid)
        {
            try { await client.DeleteAsync($"api/Sessions/{sid}"); }
            catch (HttpRequestException) { /* offline: the browser still forgets the token */ }
        }
        await localStorage.RemoveItemAsync(CardKey);
        await authStateProvider.MarkUserAsLoggedOutAsync();
    }

    public async Task<User?> GetLoggedInUser()
    {
        if (loggedInBaid == 0) return null;
        // A 401 here means the session ended; AppStatusHandler asks for the password and sends it again.
        try
        {
            var res = await client.GetAsync($"api/Users/{loggedInBaid}");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<User>() : null;
        }
        catch (HttpRequestException)
        {
            // No connection: the shell shows the bar; callers keep what they had.
            return null;
        }
    }

    public uint GetLoggedInBaid() => loggedInBaid;

    public void Dispose()
    {
        authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }

    private static string NormalizeAccessCode(string s) =>
        s.Replace(" ", "").Replace("-", "").Replace(":", "");

    private static int MapErrorMessage(string responseContent, Func<string, int> map, int fallback)
    {
        // 403/429 responses have empty or non-JSON bodies.
        Dictionary<string, string>? responseJson;
        try { responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent); }
        catch (JsonException) { return fallback; }
        if (responseJson is null || !responseJson.TryGetValue("message", out var message))
            return fallback;
        return map(message);
    }
}
