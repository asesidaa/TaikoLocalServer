using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedServerSettingsValidationTests
{
    [Fact]
    public void RedEnabledDoesNotRequireShopSettings()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Red": {
                    "Enabled": true
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration);

        var exception = Record.Exception(() =>
            provider.GetRequiredService<IOptions<ServerSettings>>().Value);

        Assert.Null(exception);

        var settings = provider.GetRequiredService<IOptions<ServerSettings>>().Value;
        Assert.True(settings.Eras[nameof(GameEra.Red)].Enabled);
        Assert.Null(settings.Eras[nameof(GameEra.Red)].EnableShop);
        Assert.Null(settings.Eras[nameof(GameEra.Red)].ActiveShopSeasonId);
    }

    private static IConfigurationRoot BuildConfiguration(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
    }

    private static ServiceProvider BuildProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddOptions<ServerSettings>()
            .Bind(configuration.GetSection("ServerSettings"))
            .ValidateStartupSettings(new HashSet<GameEra> { GameEra.Red })
            .ValidateOnStart();

        return services.BuildServiceProvider();
    }
}
