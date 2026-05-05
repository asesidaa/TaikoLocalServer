using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
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
    private readonly NavigationManager navigationManager;

    public event EventHandler? LoginStatusChanged;

    public bool LoginRequired => authConfig.AuthenticationRequired;
    public bool OnlyAdmin => authConfig.OnlyAdmin;
    public int BoundAccessCodeUpperLimit => authConfig.BoundAccessCodeUpperLimit;
    public bool RegisterWithLastPlayTime => authConfig.RegisterWithLastPlayTime;
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

    private uint loggedInBaid;

    public AuthService(
        HttpClient client,
        JwtAuthenticationStateProvider authStateProvider,
        ClientAuthConfigResponse authConfig,
        NavigationManager navigationManager)
    {
        this.client = client;
        this.authStateProvider = authStateProvider;
        this.authConfig = authConfig;
        this.navigationManager = navigationManager;

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
        loggedInBaid = isAuthenticated ? user.GetBaid() ?? 0u : 0u;
    }

    public async Task<int> Login(string inputAccessCode, string inputPassword)
    {
        var request = new LoginRequest
        {
            AccessCode = NormalizeAccessCode(inputAccessCode),
            Password = inputPassword
        };

        var responseMessage = await client.PostAsJsonAsync("api/Auth/Login", request);

        if (!responseMessage.IsSuccessStatusCode)
        {
            return MapErrorMessage(await responseMessage.Content.ReadAsStringAsync(), msg => msg switch
            {
                "Access Code Not Found" => 3,
                "User Not Registered" => 4,
                "Invalid Password" => 2,
                _ => 5
            }, fallback: 5);
        }

        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
        if (responseJson is null) return 5;

        await authStateProvider.MarkUserAsAuthenticatedAsync(responseJson["authToken"]);
        return 1;
    }

    public async Task LoginWithAuthToken()
    {
        // The state provider validates token presence and expiry on every read; we just consume
        // its result and bounce to /Login when the user holds no usable token.
        var state = await authStateProvider.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated != true && authConfig.AuthenticationRequired)
        {
            navigationManager.NavigateTo("/Login");
        }
    }

    public async Task<int> Register(string inputCardNum, DateTime inputDateTime, string inputPassword,
        string inputConfirmPassword, string inviteCode)
    {
        if (OnlyAdmin) return 0;

        if (inputPassword != inputConfirmPassword) return 2;

        var request = new RegisterRequest
        {
            AccessCode = NormalizeAccessCode(inputCardNum),
            Password = inputPassword,
            LastPlayDateTime = inputDateTime,
            InviteCode = inviteCode
        };

        var responseMessage = await client.PostAsJsonAsync("api/Auth/Register", request);
        if (responseMessage.IsSuccessStatusCode) return 1;

        return MapErrorMessage(await responseMessage.Content.ReadAsStringAsync(), msg => msg switch
        {
            "Access Code Not Found" => 3,
            "User Already Registered" => 4,
            "Wrong Last Play Time" => 5,
            _ => 6
        }, fallback: 6);
    }

    public async Task<int> ChangePassword(string inputAccessCode, string inputOldPassword, string inputNewPassword,
        string inputConfirmNewPassword)
    {
        if (OnlyAdmin) return 0;

        if (inputNewPassword != inputConfirmNewPassword) return 2;

        var request = new ChangePasswordRequest
        {
            AccessCode = inputAccessCode,
            OldPassword = inputOldPassword,
            NewPassword = inputNewPassword
        };

        var responseMessage = await client.PostAsJsonAsync("api/Auth/ChangePassword", request);
        if (responseMessage.IsSuccessStatusCode) return 1;

        return MapErrorMessage(await responseMessage.Content.ReadAsStringAsync(), msg => msg switch
        {
            "Access Code Not Found" => 3,
            "User Not Registered" => 5,
            "Wrong Old Password" => 4,
            _ => 6
        }, fallback: 6);
    }

    public Task Logout() => authStateProvider.MarkUserAsLoggedOutAsync();

    public async Task<User?> GetLoggedInUser()
    {
        if (loggedInBaid == 0) return null;
        return await client.GetFromJsonAsync<User>($"api/Users/{loggedInBaid}");
    }

    public uint GetLoggedInBaid() => loggedInBaid;

    public async Task<int> BindAccessCode(string inputAccessCode, User user)
    {
        if (string.IsNullOrWhiteSpace(inputAccessCode)) return 4;
        if (!IsLoggedIn && LoginRequired) return 0;
        var loggedInUser = await GetLoggedInUser();
        if (loggedInUser == null) return 0;
        if (LoginRequired && !IsAdmin && user.Baid != loggedInUser.Baid) return 5;
        if (user.AccessCodes.Count >= BoundAccessCodeUpperLimit) return 2;

        var request = new BindAccessCodeRequest
        {
            AccessCode = inputAccessCode,
            Baid = user.Baid
        };
        var responseMessage = await client.PostAsJsonAsync("api/Cards/BindAccessCode", request);
        return responseMessage.IsSuccessStatusCode ? 1 : 3;
    }

    public void Dispose()
    {
        authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }

    private static string NormalizeAccessCode(string s) =>
        s.Replace(" ", "").Replace("-", "").Replace(":", "");

    private static int MapErrorMessage(string responseContent, Func<string, int> map, int fallback)
    {
        var responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
        if (responseJson is null || !responseJson.TryGetValue("message", out var message))
            return fallback;
        return map(message);
    }
}
