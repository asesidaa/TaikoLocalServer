using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Contracts.AdminApi.Authorization;
using TaikoLocalServer.Infrastructure.GameDataCatalog;
using TaikoLocalServer.Infrastructure.Identity;
using TaikoLocalServer.Infrastructure.Persistence;
using TaikoLocalServer.Infrastructure.Settings;
using TaikoLocalServer.Infrastructure.Time;

namespace TaikoLocalServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Settings
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

        // Game data catalog (singleton: loaded at startup, reloaded from Server Management › Game data)
        services.AddSingleton<IGameDataCatalog, FileGameDataCatalog>();
        services.AddSingleton<IExtractRunner, ExtractRunner>();

        // Identity
        services.AddSingleton<ServerAccess>();
        services.AddSingleton<IServerAccess>(sp => sp.GetRequiredService<ServerAccess>());
        services.AddSingleton<ICabinetRegistry, CabinetRegistry>();
        services.AddSingleton<Avatars.HeadlessAvatarRenderer>();
        services.AddSingleton<IAvatarRenderer>(sp => sp.GetRequiredService<Avatars.HeadlessAvatarRenderer>());
        services.AddHostedService(sp => sp.GetRequiredService<Avatars.HeadlessAvatarRenderer>());
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddHttpContextAccessor();
        // One "Bearer" header, two kinds: tls_… is an admin's API token, anything else a Web UI sign-in (JWT).
        const string bearerOrToken = "BearerOrApiToken";
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = bearerOrToken;
            options.DefaultChallengeScheme = bearerOrToken;
        })
        .AddPolicyScheme(bearerOrToken, bearerOrToken, options => options.ForwardDefaultSelector = context =>
            ApiTokenAuthenticationHandler.IsApiToken(context.Request.Headers.Authorization)
                ? ApiTokenAuthenticationHandler.Scheme : JwtBearerDefaults.AuthenticationScheme)
        .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>(ApiTokenAuthenticationHandler.Scheme, null)
        .AddJwtBearer(options =>
        {
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
                ValidIssuer = JwtTokenService.Issuer,
                ValidAudience = JwtTokenService.Issuer,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,
            };
            options.Events = new JwtBearerEvents { OnTokenValidated = SessionValidation.OnTokenValidated };
        });
        // The signing key is generated and stored by ServerAccess; options are built on first use, after it has loaded.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IServerAccess>((options, access) =>
                options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(access.SigningKey));
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.Admin, policy => policy.RequireRole(AuthPolicies.Admin));
        });
        // Transient mirrors the framework's default registration of PolicyEvaluator;
        // the evaluator caches the IAuthorizationService it received at construction.
        services.AddTransient<IPolicyEvaluator, AuthAwarePolicyEvaluator>();

        // Time
        services.AddSingleton<IClock, Time.SystemClock>();

        return services;
    }
}
