using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Infrastructure.Identity;

namespace TaikoLocalServer.Tests.AdminApi;

// When the setup wizard runs: a fresh install until it's finished, and an install from before the wizard never.
public sealed class SetupStateTests : IAsyncLifetime
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

    private ServerAccess Access(ServerMode mode) => new(services.GetRequiredService<IServiceScopeFactory>(),
        new ConfigurationBuilder().Build(), Options.Create(new ServerSettings { Mode = mode }), NullLogger<ServerAccess>.Instance);

    [Fact]
    public async Task FreshInstall_IsPendingInBothModes_UntilFinished()
    {
        var access = Access(ServerMode.Local);
        await access.InitializeAsync();
        Assert.True(access.SetupPending);
        var code = access.SetupCode!;
        Assert.Matches("^[2-9A-Z]{4}-[2-9A-Z]{4}$", code);
        Assert.True(access.IsSetupCode(code.ToLowerInvariant()));
        Assert.True(access.IsSetupCode(code), "checking doesn't use it up");
        Assert.False(access.IsSetupCode("AAAA-AAAA"));

        await access.FinishSetupAsync(CancellationToken.None);
        Assert.False(access.SetupPending);
        Assert.False(access.IsSetupCode(code));

        // The next start: finished, in Local mode.
        var next = Access(ServerMode.Local);
        await next.InitializeAsync();
        Assert.False(next.SetupPending);
    }

    [Fact]
    public async Task ARestartTheWizardAskedFor_KeepsTheCode()
    {
        var first = Access(ServerMode.Local);
        await first.InitializeAsync();
        Environment.SetEnvironmentVariable(ServerAccess.SetupCodeVariable, first.SetupCode);

        var second = Access(ServerMode.Server);
        await second.InitializeAsync();
        Assert.Equal(first.SetupCode, second.SetupCode);
        Assert.Null(Environment.GetEnvironmentVariable(ServerAccess.SetupCodeVariable));
    }

    [Fact]
    public async Task AnInstallFromBeforeTheWizard_IsntPending_UnlessServerModeHasNoAdmin()
    {
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaikoDbContext>();
            db.ServerSecrets.Add(new ServerSecrets { JwtKey = "key", SetupFinishedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        var local = Access(ServerMode.Local);
        await local.InitializeAsync();
        Assert.False(local.SetupPending);

        // No admin to sign in with: setup opens again, for the admin account.
        var server = Access(ServerMode.Server);
        await server.InitializeAsync();
        Assert.True(server.SetupPending);
    }
}
