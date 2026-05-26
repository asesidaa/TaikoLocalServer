using Microsoft.Extensions.Configuration;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueEraFoundationTests
{
    [Fact]
    public void GameEra_Blue_HasStableNumericValue()
    {
        Assert.Equal(2, (int)GameEra.Blue);
        Assert.Equal(GameEra.Blue, Enum.Parse<GameEra>("Blue", ignoreCase: true));
    }

    [Fact]
    public void ShippedServerSettings_DeclaresBlueDisabledByDefault()
    {
        var path = FindServerSettingsPath();
        Assert.NotNull(path);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        var blue = configuration.GetSection("ServerSettings:Eras:Blue");

        Assert.True(blue.Exists());
        Assert.True(blue.GetSection("Enabled").Exists());
        Assert.False(blue.GetValue<bool>("Enabled"));
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
