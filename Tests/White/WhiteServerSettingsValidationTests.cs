using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteServerSettingsValidationTests
{
    [Fact]
    public void OptionsValidation_WhiteEnabledDoesNotRequireShopOrChallengeSettings()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "White": {
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
        Assert.True(settings.Eras[nameof(GameEra.White)].Enabled);
        Assert.Null(settings.Eras[nameof(GameEra.White)].EnableShop);
        Assert.Null(settings.Eras[nameof(GameEra.White)].ActiveShopSeasonId);
        Assert.Null(settings.Eras[nameof(GameEra.White)].EnableChallengeCompe);
        Assert.Null(settings.Eras[nameof(GameEra.White)].ActiveChallengeCompeBundleId);
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
            .ValidateStartupSettings(new HashSet<GameEra> { GameEra.White })
            .ValidateOnStart();

        return services.BuildServiceProvider();
    }
}
