using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueServerSettingsValidationTests
{
    [Fact]
    public void OptionsValidation_BlueEnabledRequiresExplicitShopEnableSetting()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Blue": {
                    "Enabled": true
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration, new HashSet<GameEra> { GameEra.Blue });

        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ServerSettings>>().Value);

        Assert.Contains(ex.Failures, failure => failure.Contains("ServerSettings:Eras:Blue:EnableShop", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_BlueShopEnabledRequiresActiveSeason()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Blue": {
                    "Enabled": true,
                    "EnableShop": true
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration, new HashSet<GameEra> { GameEra.Blue });

        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ServerSettings>>().Value);

        Assert.Contains(ex.Failures, failure => failure.Contains("ServerSettings:Eras:Blue:ActiveShopSeasonId", StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidation_BlueShopDisabledDoesNotRequireActiveSeason()
    {
        var configuration = BuildConfiguration("""
            {
              "ServerSettings": {
                "Eras": {
                  "Blue": {
                    "Enabled": true,
                    "EnableShop": false
                  }
                }
              }
            }
            """);

        using var provider = BuildProvider(configuration, new HashSet<GameEra> { GameEra.Blue });

        var settings = provider.GetRequiredService<IOptions<ServerSettings>>().Value;

        Assert.False(settings.Eras[nameof(GameEra.Blue)].EnableShop == true);
    }

    [Fact]
    public void ShippedServerSettings_DeclaresBlueCatalogSettings()
    {
        var path = FindServerSettingsPath();

        Assert.NotNull(path);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();
        var blue = configuration.GetSection("ServerSettings:Eras:Blue");

        Assert.True(blue.GetSection("AutoExtractCatalog").Exists());
        Assert.Equal("wwwroot/data/blue/data", blue.GetValue<string>("GameDataPath"));
        Assert.True(blue.GetSection("CustomizationNameDataPath").Exists());
        Assert.True(blue.GetSection("EnableShop").Exists());
        Assert.False(blue.GetValue<bool>("EnableShop"));
        Assert.Contains("\"ActiveShopSeasonId\"", File.ReadAllText(path), StringComparison.Ordinal);
    }

    private static IConfigurationRoot BuildConfiguration(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
    }

    private static ServiceProvider BuildProvider(IConfiguration configuration, ISet<GameEra> enabledEras)
    {
        var services = new ServiceCollection();
        services.AddOptions<ServerSettings>()
            .Bind(configuration.GetSection("ServerSettings"))
            .ValidateStartupSettings(enabledEras)
            .ValidateOnStart();

        return services.BuildServiceProvider();
    }

    private static string? FindServerSettingsPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "Host", "Configurations", "ServerSettings.json");
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
