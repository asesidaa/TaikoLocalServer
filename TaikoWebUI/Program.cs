using System.Globalization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using TaikoWebUI.Settings;
using TaikoWebUI.Utilities;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddMudServices();
builder.Services.AddSingleton<IGameDataService, GameDataService>();

builder.Services.Configure<WebUiSettings>(builder.Configuration.GetSection(nameof(WebUiSettings)));

builder.Services.AddScoped<LoginService>();
builder.Services.AddLocalization();
builder.Services.AddSingleton<MudLocalizer, ResXMudLocalizer>();
builder.Services.AddSingleton<ScoreUtils>();
builder.Services.AddSingleton<StringUtil>();


var host = builder.Build();

var gameDataService = host.Services.GetRequiredService<IGameDataService>();
await gameDataService.InitializeAsync(builder.HostEnvironment.BaseAddress);

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


await host.RunAsync();