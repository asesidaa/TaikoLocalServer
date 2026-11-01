using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Identity;

/// <summary>
/// Singleton behind <see cref="IServerAccess"/>. <see cref="InitializeAsync"/> runs once at startup, after
/// migrations: it creates the rules and secrets rows on first start (importing a legacy AuthSettings.json once),
/// and in Server mode without an admin prints a one-time setup code.
/// </summary>
public sealed class ServerAccess(
    IServiceScopeFactory scopes,
    IConfiguration configuration,
    IOptions<ServerSettings> serverSettings,
    ILogger<ServerAccess> logger) : IServerAccess
{
    /// <summary>The key every install shipped with before keys were generated; never kept on import.</summary>
    private const string LegacyDefaultJwtKey = "SuperSecretKeyAndHeresItsPadding";

    private AccessRules rules = new();
    private byte[] signingKey = [];
    private string? setupCode;

    public ServerMode Mode { get; private set; }

    public AccessRules Rules => Copy(rules);

    public byte[] SigningKey => signingKey.Length > 0 ? signingKey : throw new InvalidOperationException("ServerAccess isn't initialized.");

    public bool SetupPending => setupCode is not null;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // The pre-Mode switch: AuthSettings.json's AuthenticationRequired. Only consulted when Mode is unset.
        var legacy = configuration.GetSection("AuthSettings");
        Mode = serverSettings.Value.Mode ?? (legacy.GetValue<bool>("AuthenticationRequired") ? ServerMode.Server : ServerMode.Local);

        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();

        if (await db.AccessRules.FindAsync([1], cancellationToken) is { } stored)
        {
            rules = stored;
        }
        else
        {
            // First start: carry over a legacy AuthSettings.json's rules once; afterwards they're edited in Admin › Access.
            rules = new AccessRules
            {
                OnlyAdmin = legacy.GetValue("OnlyAdmin", false),
                RegisterWithLastPlayTime = legacy.GetValue("RegisterWithLastPlayTime", false),
                AllowUserDelete = legacy.GetValue("AllowUserDelete", true),
                AllowFreeProfileEditing = legacy.GetValue("AllowFreeProfileEditing", true),
                AccessCodeLimit = legacy.GetValue("BoundAccessCodeUpperLimit", 3),
            };
            db.AccessRules.Add(rules);
        }

        if (await db.ServerSecrets.FindAsync([1], cancellationToken) is not { } secrets)
        {
            // Keep an operator's own legacy key (existing logins stay valid); replace the shipped default.
            var legacyKey = legacy["JwtKey"];
            secrets = new ServerSecrets
            {
                JwtKey = !string.IsNullOrEmpty(legacyKey) && legacyKey != LegacyDefaultJwtKey
                    ? legacyKey
                    : Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            };
            db.ServerSecrets.Add(secrets);
        }
        signingKey = Encoding.UTF8.GetBytes(secrets.JwtKey);

        await db.SaveChangesAsync(cancellationToken);

        if (Mode == ServerMode.Server && !await db.UserData.AnyAsync(u => u.IsAdmin, cancellationToken))
        {
            setupCode = NewSetupCode();
            logger.LogWarning("Server mode has no admin yet. Register on the WebUI with setup code {SetupCode} to become the admin " +
                              "(or run: TaikoLocalServer admin promote <access code>). The code changes on every start until an admin exists.", setupCode);
        }
        logger.LogInformation("Mode: {Mode}", Mode);
    }

    public async Task UpdateRulesAsync(AccessRules updated, CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        var row = await db.AccessRules.FindAsync([1], cancellationToken) ?? db.AccessRules.Add(new AccessRules()).Entity;
        row.OnlyAdmin = updated.OnlyAdmin;
        row.RegisterWithLastPlayTime = updated.RegisterWithLastPlayTime;
        row.AllowUserDelete = updated.AllowUserDelete;
        row.AllowFreeProfileEditing = updated.AllowFreeProfileEditing;
        row.AccessCodeLimit = Math.Max(1, updated.AccessCodeLimit);
        await db.SaveChangesAsync(cancellationToken);
        rules = Copy(row);
    }

    public bool TryUseSetupCode(string code)
    {
        var expected = setupCode;
        if (expected is null || string.IsNullOrWhiteSpace(code))
            return false;
        var given = Encoding.UTF8.GetBytes(code.Trim().ToUpperInvariant());
        if (!CryptographicOperations.FixedTimeEquals(given, Encoding.UTF8.GetBytes(expected)))
            return false;
        setupCode = null;
        return true;
    }

    // XXXX-XXXX from letters and digits that can't be confused with each other (no 0/O, 1/I/L).
    private static string NewSetupCode()
    {
        const string alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
        var chars = Enumerable.Range(0, 8).Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]).ToArray();
        return $"{new string(chars, 0, 4)}-{new string(chars, 4, 4)}";
    }

    private static AccessRules Copy(AccessRules r) => new()
    {
        OnlyAdmin = r.OnlyAdmin, RegisterWithLastPlayTime = r.RegisterWithLastPlayTime, AllowUserDelete = r.AllowUserDelete,
        AllowFreeProfileEditing = r.AllowFreeProfileEditing, AccessCodeLimit = r.AccessCodeLimit,
    };
}
