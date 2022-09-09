using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaikoWebUI;
using MudBlazor.Services;
using TaikoWebUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration.GetValue<string>("BaseUrl"))
});
builder.Services.AddMudServices();
builder.Services.AddSingleton<IGameDataService, GameDataService>();

var host = builder.Build();

var gameDataService = host.Services.GetRequiredService<IGameDataService>();
await gameDataService.InitializeAsync(builder.Configuration.GetValue<string>("DataBaseUrl"));

await host.RunAsync();