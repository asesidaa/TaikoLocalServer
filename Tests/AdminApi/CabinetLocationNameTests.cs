using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Infrastructure.Identity;

namespace TaikoLocalServer.Tests.AdminApi;

// The location name a cabinet reports at start-up (Garm RegisterSystemBoard, before its game requests): kept for the
// Cabinets page, without adding the cabinet, since that would let it skip approval when RestrictChassisIds is on.
public sealed class CabinetLocationNameTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private ServiceProvider services = null!;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        services = new ServiceCollection()
            .AddDbContext<TaikoDbContext>(o => o.UseSqlite(connection))
            .AddScoped<ITaikoDbContext>(sp => sp.GetRequiredService<TaikoDbContext>())
            .BuildServiceProvider();
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TaikoDbContext>().Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await services.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task ANameBeforeTheFirstGameRequest_IsKeptForIt_WithoutAddingTheCabinet()
    {
        var registry = new CabinetRegistry(services.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new ServerSettings { RestrictChassisIds = true }), NullLogger<CabinetRegistry>.Instance);

        registry.Located("284111080000", "  TAIKO ARCADE LOADER ");
        Assert.Null(registry.Find("284111080000"));

        Assert.False(registry.Seen("284111080000", "JPN0JPN0FF0", "WW 39.06"));
        var cabinet = registry.Find("284111080000")!;
        Assert.Equal(CabinetAccess.Pending, cabinet.Access);
        Assert.Equal("TAIKO ARCADE LOADER", cabinet.LocationName);

        registry.Located("284111080000", "Arcade 2");
        Assert.Equal("Arcade 2", registry.Find("284111080000")!.LocationName);

        // Written through: a restart reads it back.
        for (var i = 0; i < 50 && await Stored() != "Arcade 2"; i++) await Task.Delay(20);
        Assert.Equal("Arcade 2", await Stored());
    }

    private async Task<string?> Stored()
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaikoDbContext>();
        return (await db.Cabinets.AsNoTracking().FirstOrDefaultAsync(c => c.ChassisId == "284111080000"))?.LocationName;
    }
}
