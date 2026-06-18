using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Contracts.AdminApi.Authorization;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Nijiiro;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Red;
using TaikoLocalServer.Infrastructure.GameDataCatalog.White;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Settings;
using TaikoLocalServer.Infrastructure.Identity;
using TaikoLocalServer.Infrastructure.Identity.Settings;
using TaikoLocalServer.Infrastructure.Persistence;
using TaikoLocalServer.Infrastructure.Settings;
using TaikoLocalServer.Infrastructure.Time;

namespace TaikoLocalServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        ISet<GameEra> enabledEras)
    {
        // Settings
        services.Configure<AuthSettings>(configuration.GetSection(nameof(AuthSettings)));
        services.Configure<DataSettings>(configuration.GetSection(nameof(DataSettings)));
        services.AddOptions<ServerSettings>()
            .Bind(configuration.GetSection(nameof(ServerSettings)))
            .ValidateStartupSettings(enabledEras)
            .ValidateOnStart();
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
        if (enabledEras.Contains(GameEra.Nijiiro))
        {
            services.AddSingleton<NijiiroEraGameDataCatalog>();
            services.AddSingleton<INijiiroCatalog>(sp => sp.GetRequiredService<NijiiroEraGameDataCatalog>());
            services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<NijiiroEraGameDataCatalog>());
        }

        if (enabledEras.Contains(GameEra.Green))
        {
            services.AddSingleton<GreenEraGameDataCatalog>();
            services.AddSingleton<IGreenCatalog>(sp => sp.GetRequiredService<GreenEraGameDataCatalog>());
            services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<GreenEraGameDataCatalog>());
        }

        if (enabledEras.Contains(GameEra.Blue))
        {
            services.AddSingleton<BlueEraGameDataCatalog>();
            services.AddSingleton<IBlueCatalog>(sp => sp.GetRequiredService<BlueEraGameDataCatalog>());
            services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<BlueEraGameDataCatalog>());
        }

        if (enabledEras.Contains(GameEra.Yellow))
        {
            services.AddSingleton<YellowEraGameDataCatalog>();
            services.AddSingleton<IYellowCatalog>(sp => sp.GetRequiredService<YellowEraGameDataCatalog>());
            services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<YellowEraGameDataCatalog>());
        }

        if (enabledEras.Contains(GameEra.Red))
        {
            services.AddSingleton<RedEraGameDataCatalog>();
            services.AddSingleton<IRedCatalog>(sp => sp.GetRequiredService<RedEraGameDataCatalog>());
            services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<RedEraGameDataCatalog>());
        }

        if (enabledEras.Contains(GameEra.White))
        {
            services.AddSingleton<WhiteEraGameDataCatalog>();
            services.AddSingleton<IWhiteCatalog>(sp => sp.GetRequiredService<WhiteEraGameDataCatalog>());
            services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<WhiteEraGameDataCatalog>());
        }

        services.AddSingleton<IGameDataCatalog>(sp => new FileGameDataCatalog(
            sp.GetServices<IEraGameDataCatalog>()));

        // Identity
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddHttpContextAccessor();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var authSection = configuration.GetSection(nameof(AuthSettings));
            // MapInboundClaims = true ensures the JWT short names (unique_name, role)
            // are mapped to the long-form ClaimTypes URIs on the ClaimsPrincipal so
            // User.IsInRole("Admin") and User.FindFirstValue(ClaimTypes.Name) work.
            options.MapInboundClaims = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authSection["JwtIssuer"],
                ValidAudience = authSection["JwtAudience"],
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSection["JwtKey"] ?? throw new InvalidOperationException()))
            };
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.Admin, policy => policy.RequireRole(AuthPolicies.Admin));
        });
        // Transient mirrors the framework's default registration of PolicyEvaluator;
        // the evaluator caches the IAuthorizationService it received at construction.
        services.AddTransient<IPolicyEvaluator, AuthAwarePolicyEvaluator>();

        // Time
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
