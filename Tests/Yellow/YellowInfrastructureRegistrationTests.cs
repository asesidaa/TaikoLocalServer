using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Infrastructure;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowInfrastructureRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersYellowCatalogWhenYellowEnabled()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();
        services.AddLogging();

        services.AddInfrastructure(configuration, new HashSet<GameEra> { GameEra.Yellow });
        using var provider = services.BuildServiceProvider();

        Assert.IsType<YellowEraGameDataCatalog>(provider.GetRequiredService<IYellowCatalog>());
        var gameDataCatalog = provider.GetRequiredService<IGameDataCatalog>();
        Assert.Equal(GameEra.Yellow, gameDataCatalog.For(GameEra.Yellow).Era);
    }

    [Fact]
    public void AddInfrastructure_DoesNotRegisterYellowCatalogWhenYellowDisabled()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();
        services.AddLogging();

        services.AddInfrastructure(configuration, new HashSet<GameEra> { GameEra.Green });
        using var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<IYellowCatalog>());
        var gameDataCatalog = provider.GetRequiredService<IGameDataCatalog>();
        var ex = Assert.Throws<InvalidOperationException>(() => gameDataCatalog.For(GameEra.Yellow));
        Assert.Contains("Era Yellow is not enabled", ex.Message, StringComparison.Ordinal);
    }

    private static IConfigurationRoot BuildConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbFileName"] = "test.db3",
                ["AuthSettings:JwtKey"] = "0123456789abcdef0123456789abcdef",
                ["AuthSettings:JwtIssuer"] = "tests",
                ["AuthSettings:JwtAudience"] = "tests",
                ["ServerSettings:Eras:Yellow:Enabled"] = "true",
                ["ServerSettings:Eras:Yellow:EnableShop"] = "false",
                ["ServerSettings:Eras:Yellow:GameDataPath"] = "wwwroot/data/yellow/data",
                ["ServerSettings:Eras:Green:Enabled"] = "true",
                ["ServerSettings:Eras:Green:EnableShop"] = "false"
            })
            .Build();
}
