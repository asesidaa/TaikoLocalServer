using TaikoLocalServer.Adapters.AdminApi.Authorization;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.AdminApi;

public sealed class UserGamesTests
{
    [Fact]
    public async Task ForAsync_ListsEachGameWithASaveOrPlays_NewestFirst()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new TaikoDbContext(new DbContextOptionsBuilder<TaikoDbContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        context.UserData.AddRange(new UserDatum { Baid = 1 }, new UserDatum { Baid = 2 });
        context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        context.SongBestDataRed.AddRange(
            new SongBestDatumRed { Baid = 1, SongId = 5, Difficulty = Difficulty.Oni, BestScore = 900_000 },
            new SongBestDatumRed { Baid = 1, SongId = 5, Difficulty = Difficulty.Oni, IsShin = true, BestScore = 950_000 });
        context.SongPlayDataRed.AddRange(
            new SongPlayDatumRed { Baid = 1, SongId = 5, Difficulty = Difficulty.Oni, PlayTime = new DateTime(2026, 10, 1) },
            new SongPlayDatumRed { Baid = 1, SongId = 5, Difficulty = Difficulty.Oni, PlayTime = new DateTime(2026, 10, 3) });
        await context.SaveChangesAsync();

        var games = await UserGames.ForAsync(context, [1u, 2u], CancellationToken.None);

        Assert.Equal([new UserGame("Red", 2, new DateTime(2026, 10, 3)), new UserGame("Green", 0, null)], games[1]);
        Assert.Empty(games[2]);

        // Both rows of a song with a Shin chart go, with the plays behind them.
        Assert.Equal((950_000u, 2), await UserGames.RemoveScoreAsync(context, GameEra.Red, 1, 5, Difficulty.Oni, CancellationToken.None));
        await context.SaveChangesAsync();
        Assert.False(await context.SongBestDataRed.AnyAsync());
        Assert.False(await context.SongPlayDataRed.AnyAsync());
        Assert.Null(await UserGames.RemoveScoreAsync(context, GameEra.Red, 1, 5, Difficulty.Oni, CancellationToken.None));
    }
}
