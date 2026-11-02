using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using TaikoLocalServer.Contracts.AdminApi.Authorization;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoWebUI.Authorization;
using TaikoWebUI.Localization;
using TaikoWebUI.Services;
using TaikoWebUI.Settings;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Single HttpClient reused for both bootstrap fetches and the registered DI singleton. Every call reports the
// connection, sign-outs and the server state to AppStatus (the System states).
var appStatus = new AppStatus();
var httpClient = new HttpClient(new AppStatusHandler(appStatus) { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};

// Bootstrap: fetch the local appsettings.json AND the server's auth policy concurrently.
// Buffer appsettings.json into memory because Blazor WASM's HttpClient stream is fetch-API-backed
// and rejects the synchronous reads AddJsonStream/Build performs internally.
var configurationBytesTask = httpClient.GetByteArrayAsync("appsettings.json");
var authConfigTask = httpClient.GetFromJsonAsync<ClientAuthConfigResponse>("api/Auth/Config");
var serverStateTask = httpClient.GetFromJsonAsync<ServerStateSettings>("api/ServerState/Current");
await Task.WhenAll(configurationBytesTask, authConfigTask, serverStateTask);
appStatus.Start(httpClient, await serverStateTask);

var configurationBytes = await configurationBytesTask;
var authConfig = await authConfigTask
    ?? throw new InvalidOperationException("Server returned null ClientAuthConfigResponse from /api/Auth/Config.");

using var configurationStream = new MemoryStream(configurationBytes);
var configuration = new ConfigurationBuilder()
    .AddJsonStream(configurationStream)
    .Build();

builder.Services.AddSingleton(httpClient);
builder.Services.AddSingleton(appStatus);
builder.Services.AddSingleton(authConfig);
builder.Services.AddSingleton<GameDataService>();

builder.Services.Configure<WebUiSettings>(configuration.GetSection(nameof(WebUiSettings)));

// Authorization policies mirror the server's AuthAwarePolicyEvaluator: when authentication is
// not required, every policy passes uniformly (synthetic admin principal in the state provider
// makes role checks succeed too). RequireRealAuth is the inverse — used to gate the login /
// register / change-password pages so they redirect away in local mode.
builder.Services.AddAuthorizationCore(options =>
{
    if (authConfig.AuthenticationRequired)
    {
        options.AddPolicy(AuthPolicies.Admin, p => p.RequireRole(AuthPolicies.Admin));
        // RequireRealAuth means "the system has auth turned on", not "the current user is logged in".
        // Login/Register pages use this alone so anonymous visitors can reach them in auth-required mode;
        // ChangePassword stacks [Authorize] on top to require an actual logged-in user.
        options.AddPolicy(WebUiAuthPolicies.RequireRealAuth, p => p.RequireAssertion(_ => true));
    }
    else
    {
        var allowAll = new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();
        options.AddPolicy(AuthPolicies.Admin, p => p.RequireAssertion(_ => true));
        // RequireRealAuth fails in local mode -> AuthorizeRouteView's <NotAuthorized> fires
        // -> RedirectToLogin sends the user to "/" since there's nothing to log in to.
        options.AddPolicy(WebUiAuthPolicies.RequireRealAuth, p => p.RequireAssertion(_ => false));
        options.DefaultPolicy = allowAll;
        options.FallbackPolicy = allowAll;
    }
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SongListState>();
builder.Services.AddScoped<OverlayHost>();
builder.Services.AddLocalization();
builder.Services.AddScoped<PageState>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<AssetStatus>();
builder.Services.AddScoped<PlayerSelection>();
builder.Services.AddScoped<AdminView>();
builder.Services.AddBlazoredLocalStorage();

var host = builder.Build();

CultureInfo culture;
var js = host.Services.GetRequiredService<IJSRuntime>();
var result = await js.InvokeAsync<string?>("blazorCulture.get");

if (result is not null)
{
    culture = new CultureInfo(result);
}
else
{
    culture = new CultureInfo("en-US");
    await js.InvokeVoidAsync("blazorCulture.set", "en-US");
}

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var labels = host.Services.GetRequiredService<IStringLocalizer<LocalizationResource>>();
GameData.T = s => labels[s];

await host.RunAsync();
