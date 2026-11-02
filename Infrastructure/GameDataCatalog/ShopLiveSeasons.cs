using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

/// <summary>The live shop seasons in memory (the catalogs read them as they load), written through to ShopLiveSeasons.</summary>
public sealed class ShopLiveSeasons(IServiceScopeFactory scopes) : IShopLiveSeasons
{
    private readonly ConcurrentDictionary<GameEra, uint> seasons = new();

    public async Task InitializeAsync()
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        foreach (var row in await db.ShopLiveSeasons.AsNoTracking().ToListAsync())
        {
            if (Enum.TryParse<GameEra>(row.Era, out var era)) seasons[era] = row.SeasonId;
        }
    }

    public uint? Get(GameEra era) => seasons.TryGetValue(era, out var id) ? id : null;

    public async Task SetAsync(GameEra era, uint seasonId, CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        var row = await db.ShopLiveSeasons.FindAsync([era.ToString()], cancellationToken);
        if (row is null) db.ShopLiveSeasons.Add(row = new ShopLiveSeason { Era = era.ToString() });
        row.SeasonId = seasonId;
        await db.SaveChangesAsync(cancellationToken);
        seasons[era] = seasonId;
    }
}
