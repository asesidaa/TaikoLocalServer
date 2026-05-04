using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Infrastructure.GameDataCatalog;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Settings;
using TaikoLocalServer.Infrastructure.Identity;
using TaikoLocalServer.Infrastructure.Identity.Settings;
using TaikoLocalServer.Infrastructure.Persistence;
using TaikoLocalServer.Infrastructure.Settings;
using TaikoLocalServer.Infrastructure.Time;

namespace TaikoLocalServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Settings
        services.Configure<AuthSettings>(configuration.GetSection(nameof(AuthSettings)));
        services.Configure<DataSettings>(configuration.GetSection(nameof(DataSettings)));
        services.Configure<ServerSettings>(configuration.GetSection(nameof(ServerSettings)));   // Application's portion (EnableMoreSongs / MoreSongsSize)
        services.Configure<AllnetSettings>(configuration.GetSection(nameof(ServerSettings)));   // Infrastructure's portion (MuchaUrl / GameUrl) — same JSON section, different POCO

        // Persistence
        services.AddDbContext<TaikoDbContext>(option =>
        {
            var dbName = configuration["DbFileName"];
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = PersistenceConstants.DefaultDbName;
            }

            var path = Path.Combine(PathHelper.GetRootPath(), dbName);
            option
                .UseSqlite($"Data Source={path}")
                .ConfigureWarnings(warnings => warnings
                    .Ignore(RelationalEventId.NonTransactionalMigrationOperationWarning)
                    .Ignore(SqliteEventId.TableRebuildPendingWarning));
        });
        services.AddScoped<ITaikoDbContext>(sp => sp.GetRequiredService<TaikoDbContext>());

        // Game data catalog (singleton — initialized once at startup)
        services.AddSingleton<IGameDataCatalog, FileGameDataCatalog>();

        // Identity
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var authSection = configuration.GetSection(nameof(AuthSettings));
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authSection["JwtIssuer"],
                ValidAudience = authSection["JwtAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSection["JwtKey"] ?? throw new InvalidOperationException()))
            };
        });

        // Time
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
