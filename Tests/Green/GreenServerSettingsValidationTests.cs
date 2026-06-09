using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenServerSettingsValidationTests
{
    [Fact]
    public void OptionsValidation_GreenEnabledRequiresExplicitShopEnableSetting()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Green": {
                    "Enabled": true
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration);

        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ServerSettings>>().Value);

        Assert.Contains(ex.Failures, failure => failure.Contains("ServerSettings:Eras:Green:EnableShop", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_GreenShopEnabledRequiresActiveSeason()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Green": {
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

        Assert.Contains(ex.Failures, failure => failure.Contains("ServerSettings:Eras:Green:ActiveShopSeasonId", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_GreenShopDisabledDoesNotRequireActiveSeason()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Green": {
                    "Enabled": true,
                    "EnableShop": false
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration);

        var settings = provider.GetRequiredService<IOptions<ServerSettings>>().Value;

        Assert.False(settings.Eras[nameof(GameEra.Green)].EnableShop == true);
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
            .ValidateStartupSettings(GreenEnabledEras())
            .ValidateOnStart();

        return services.BuildServiceProvider();
    }

    private static ISet<GameEra> GreenEnabledEras()
        => new HashSet<GameEra> { GameEra.Green };

}
