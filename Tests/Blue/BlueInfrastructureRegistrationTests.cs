using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueInfrastructureRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersBlueCatalogWhenBlueEnabled()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();
        services.AddLogging();

        services.AddInfrastructure(configuration, new HashSet<GameEra> { GameEra.Blue });
        using var provider = services.BuildServiceProvider();

        Assert.IsType<BlueEraGameDataCatalog>(provider.GetRequiredService<IBlueCatalog>());
        var gameDataCatalog = provider.GetRequiredService<IGameDataCatalog>();
        Assert.Equal(GameEra.Blue, gameDataCatalog.For(GameEra.Blue).Era);
    }

    [Fact]
    public void AddInfrastructure_DoesNotRegisterBlueCatalogWhenBlueDisabled()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();
        services.AddLogging();

        services.AddInfrastructure(configuration, new HashSet<GameEra> { GameEra.Green });
        using var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<IBlueCatalog>());
        var gameDataCatalog = provider.GetRequiredService<IGameDataCatalog>();
        var ex = Assert.Throws<InvalidOperationException>(() => gameDataCatalog.For(GameEra.Blue));
        Assert.Contains("Era Blue is not enabled", ex.Message, StringComparison.Ordinal);
    }

    private static IConfigurationRoot BuildConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbFileName"] = "test.db3",
                ["AuthSettings:JwtKey"] = "0123456789abcdef0123456789abcdef",
                ["AuthSettings:JwtIssuer"] = "tests",
                ["AuthSettings:JwtAudience"] = "tests",
                ["ServerSettings:Eras:Blue:Enabled"] = "true",
                ["ServerSettings:Eras:Blue:EnableShop"] = "false",
                ["ServerSettings:Eras:Green:Enabled"] = "true",
                ["ServerSettings:Eras:Green:EnableShop"] = "false"
            })
            .Build();
}
