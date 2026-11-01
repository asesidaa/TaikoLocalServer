using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Identity;

/// <summary>
/// Cabinets in memory (the access check runs on every game request), written through to the Cabinets table:
/// a new cabinet at once, a known one's "last seen" at most once a minute.
/// </summary>
public sealed class CabinetRegistry(IServiceScopeFactory scopes, IOptions<ServerSettings> settings, ILogger<CabinetRegistry> logger)
    : ICabinetRegistry
{
    private static readonly TimeSpan SeenGranularity = TimeSpan.FromMinutes(1);
    private readonly ConcurrentDictionary<string, Cabinet> cabinets = new();
    private readonly SemaphoreSlim writes = new(1, 1);

    public bool Restricted => settings.Value.RestrictChassisIds;

    public async Task InitializeAsync()
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        foreach (var c in await db.Cabinets.AsNoTracking().ToListAsync())
            cabinets[c.ChassisId] = c;
    }

    public bool Seen(string chassisId, string shopId, string version)
    {
        if (string.IsNullOrWhiteSpace(chassisId)) return !Restricted;
        var now = DateTime.UtcNow;
        var isNew = false;
        var cabinet = cabinets.GetOrAdd(chassisId, id =>
        {
            isNew = true;
            return new Cabinet { ChassisId = id, FirstSeen = now, Access = Restricted ? CabinetAccess.Pending : CabinetAccess.Allowed };
        });

        var allowed = cabinet.Access == CabinetAccess.Allowed;
        var write = isNew || !allowed || cabinet.LastSeen is null || now - cabinet.LastSeen > SeenGranularity
                    || cabinet.ShopId != shopId || cabinet.Version != version;
        cabinet.FirstSeen ??= now;
        cabinet.LastSeen = now;
        cabinet.ShopId = shopId;
        cabinet.Version = version;
        if (!allowed) cabinet.RefusedCount++;
        if (isNew) logger.LogInformation("New cabinet {ChassisId} (shop {ShopId}, {Version}){Pending}", chassisId, shopId, version,
            allowed ? "" : ": waiting for approval");
        if (write) _ = SaveAsync(chassisId, CancellationToken.None);
        return allowed;
    }

    public IReadOnlyList<Cabinet> List() => cabinets.Values.OrderByDescending(c => c.LastSeen).ToList();

    public Cabinet? Find(string chassisId) => cabinets.GetValueOrDefault(chassisId);

    public async Task UpdateAsync(string chassisId, Action<Cabinet> change, CancellationToken cancellationToken)
    {
        var cabinet = cabinets.GetOrAdd(chassisId, id => new Cabinet { ChassisId = id });
        change(cabinet);
        await SaveAsync(chassisId, cancellationToken);
    }

    public async Task<bool> RemoveAsync(string chassisId, CancellationToken cancellationToken)
    {
        if (!cabinets.TryRemove(chassisId, out _)) return false;
        await writes.WaitAsync(cancellationToken);
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
            await db.Cabinets.Where(c => c.ChassisId == chassisId).ExecuteDeleteAsync(cancellationToken);
            return true;
        }
        finally
        {
            writes.Release();
        }
    }

    private async Task SaveAsync(string chassisId, CancellationToken cancellationToken)
    {
        if (!cabinets.TryGetValue(chassisId, out var cabinet)) return;
        await writes.WaitAsync(cancellationToken);
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
            var row = await db.Cabinets.FindAsync([chassisId], cancellationToken);
            if (row is null) db.Cabinets.Add(row = new Cabinet { ChassisId = chassisId });
            row.ShopId = cabinet.ShopId;
            row.Version = cabinet.Version;
            row.FirstSeen = cabinet.FirstSeen;
            row.LastSeen = cabinet.LastSeen;
            row.Access = cabinet.Access;
            row.RefusedCount = cabinet.RefusedCount;
            row.OwnSettings = cabinet.OwnSettings;
            row.DanSet = cabinet.DanSet;
            row.Movies = cabinet.Movies;
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e) when (e is DbUpdateException or InvalidOperationException)
        {
            logger.LogError(e, "Couldn't save cabinet {ChassisId}", chassisId);
        }
        finally
        {
            writes.Release();
        }
    }
}
