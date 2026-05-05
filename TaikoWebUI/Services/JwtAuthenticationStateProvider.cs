using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using TaikoLocalServer.Contracts.AdminApi.Authorization;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoWebUI.Services;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string TokenStorageKey = "authToken";
    private const string LocalAuthenticationType = "Local";
    private const string TokenAuthenticationType = "Bearer";

    // Reused: ReadJwtToken is allocation-friendly but the handler itself holds settings.
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ClientAuthConfigResponse config;
    private readonly ILocalStorageService localStorage;
    private readonly HttpClient httpClient;

    public JwtAuthenticationStateProvider(
        ClientAuthConfigResponse config,
        ILocalStorageService localStorage,
        HttpClient httpClient)
    {
        this.config = config;
        this.localStorage = localStorage;
        this.httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!config.AuthenticationRequired)
        {
            // Local mode: synthesize an admin principal so <AuthorizeView Roles="Admin"> blocks
            // unfold and User.IsAdmin() evaluates true. This mirrors the server-side
            // AuthAwarePolicyEvaluator behavior where every policy passes.
            var localIdentity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "local"),
                    new Claim(ClaimTypes.Role, AuthPolicies.Admin),
                },
                authenticationType: LocalAuthenticationType,
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);
            return new AuthenticationState(new ClaimsPrincipal(localIdentity));
        }

        var token = await localStorage.GetItemAsync<string>(TokenStorageKey);
        if (string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
            return AnonymousState;
        }

        JwtSecurityToken jwt;
        try
        {
            jwt = TokenHandler.ReadJwtToken(token);
        }
        catch
        {
            await localStorage.RemoveItemAsync(TokenStorageKey);
            httpClient.DefaultRequestHeaders.Authorization = null;
            return AnonymousState;
        }

        if (jwt.ValidTo <= DateTime.UtcNow)
        {
            await localStorage.RemoveItemAsync(TokenStorageKey);
            httpClient.DefaultRequestHeaders.Authorization = null;
            return AnonymousState;
        }

        // ReadJwtToken returns raw short-form claims (unique_name, role). Map them to
        // the long-form ClaimTypes URIs so the moved ClaimsPrincipalExtensions
        // (FindFirst(ClaimTypes.Name) / IsInRole) work uniformly across server and client.
        var claims = new List<Claim>(jwt.Claims.Count());
        foreach (var c in jwt.Claims)
        {
            var type = c.Type switch
            {
                JwtRegisteredClaimNames.UniqueName => ClaimTypes.Name,
                "role" => ClaimTypes.Role,
                _ => c.Type
            };
            claims.Add(new Claim(type, c.Value));
        }
        var identity = new ClaimsIdentity(
            claims,
            authenticationType: TokenAuthenticationType,
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await localStorage.SetItemAsync(TokenStorageKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await localStorage.RemoveItemAsync(TokenStorageKey);
        httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
