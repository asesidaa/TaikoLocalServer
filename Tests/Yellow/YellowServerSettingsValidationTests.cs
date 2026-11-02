using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowServerSettingsValidationTests
{
    [Fact]
    public void OptionsValidation_YellowEnabledRequiresExplicitShopEnableSetting()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Yellow": {
                    "Enabled": true
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration);

        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ServerSettings>>().Value);

        Assert.Contains(ex.Failures, failure => failure.Contains("ServerSettings:Eras:Yellow:EnableShop", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_YellowShopEnabledRequiresActiveSeason()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Yellow": {
                    "Enabled": true,
                    "EnableShop": true
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration);

        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ServerSettings>>().Value);

        Assert.Contains(ex.Failures, failure => failure.Contains("ServerSettings:Eras:Yellow:ActiveShopSeasonId", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_YellowShopDisabledDoesNotRequireActiveSeason()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Yellow": {
                    "Enabled": true,
                    "EnableShop": false
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration);

        var settings = provider.GetRequiredService<IOptions<ServerSettings>>().Value;

        Assert.False(settings.Eras[nameof(GameEra.Yellow)].EnableShop == true);
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
            .ValidateStartupSettings(new HashSet<GameEra> { GameEra.Yellow })
            .ValidateOnStart();

        return services.BuildServiceProvider();
    }

}
