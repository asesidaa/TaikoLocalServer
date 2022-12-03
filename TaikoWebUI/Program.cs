using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TaikoWebUI.Settings;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddMudServices();
builder.Services.AddSingleton<IGameDataService, GameDataService>();
builder.Services.AddScoped<LoginService>();

var http = new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};
using var response = await http.GetAsync("WebUiSettings.json");
await using var stream = await response.Content.ReadAsStreamAsync();
builder.Configuration.AddJsonStream(stream);
builder.Services.Configure<WebUiSettings>(builder.Configuration.GetSection(nameof(WebUiSettings)));

var host = builder.Build();

var gameDataService = host.Services.GetRequiredService<IGameDataService>();
await gameDataService.InitializeAsync(builder.HostEnvironment.BaseAddress);

await host.RunAsync();