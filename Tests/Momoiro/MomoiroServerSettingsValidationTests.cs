using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroServerSettingsValidationTests
{
    [Fact]
    public void OptionsValidation_MomoiroEnabledDoesNotRequireShopOrChallengeSettings()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Momoiro": {
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
        Assert.True(settings.Eras["Momoiro"].Enabled);
        Assert.Null(settings.Eras["Momoiro"].EnableShop);
        Assert.Null(settings.Eras["Momoiro"].ActiveShopSeasonId);
        Assert.Null(settings.Eras["Momoiro"].EnableDonChallenge);
        Assert.Null(settings.Eras["Momoiro"].ActiveDonChallengeBundleId);
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
        var enabledEras = new HashSet<GameEra>
        {
            Enum.Parse<GameEra>("Momoiro", ignoreCase: true)
        };

        var services = new ServiceCollection();
        services.AddOptions<ServerSettings>()
            .Bind(configuration.GetSection("ServerSettings"))
            .ValidateStartupSettings(enabledEras)
            .ValidateOnStart();

        return services.BuildServiceProvider();
    }
}
