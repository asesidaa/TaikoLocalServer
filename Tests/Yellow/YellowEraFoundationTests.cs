using TaikoLocalServer.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowEraFoundationTests
{
    [Fact]
    public void GameEra_Yellow_HasStableNumericValue()
    {
        var parsed = Enum.Parse<GameEra>("Yellow", ignoreCase: true);

        Assert.Equal(3, (int)parsed);
    }

    [Fact]
    public void ExistingEraNumericValues_RemainStable()
    {
        Assert.Equal(0, (int)GameEra.Nijiiro);
        Assert.Equal(1, (int)GameEra.Green);
        Assert.Equal(2, (int)GameEra.Blue);
    }

    [Fact]
    public void ShippedServerSettings_DeclaresYellowEnabledSettingAndDataPath()
    {
        var path = FindServerSettingsPath();
        Assert.NotNull(path);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        var yellow = configuration.GetSection("ServerSettings:Eras:Yellow");
        var enabled = yellow.GetSection("Enabled").Value;

        Assert.True(yellow.Exists());
        Assert.True(yellow.GetSection("Enabled").Exists());
        Assert.True(bool.TryParse(enabled, out _));
        Assert.Equal("wwwroot/data/yellow/data", yellow.GetValue<string>("GameDataPath"));
    }

    private static string? FindServerSettingsPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(
                directory.FullName,
                "Host",
                "Configurations",
                "ServerSettings.json");

            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
