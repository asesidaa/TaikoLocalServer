using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15NormalPlayServiceTests
{
    [Fact]
    public async Task SaveAsync_ReturnsSuccessWhenBaidIsZero()
    {
        var result = await Ac15NormalPlayService.SaveAsync(
            baid: 0,
            new CommonPlayResultData(),
            Ac15EraProfiles.Blue,
            new FakePersistence(userExists: false),
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
    }

    [Fact]
    public async Task SaveAsync_SkipsMissingUserWithSuccess()
    {
        var persistence = new FakePersistence(userExists: false);

        var result = await Ac15NormalPlayService.SaveAsync(
            baid: 1,
            new CommonPlayResultData(),
            Ac15EraProfiles.Blue,
            persistence,
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.False(persistence.SaveWasCalled);
    }

    [Fact]
    public async Task SaveAsync_AddsStageRowsAndTrimsRecentSongs()
    {
        var persistence = new FakePersistence(userExists: true);
        var playResult = new CommonPlayResultData
        {
            PlayDatetime = "20260607010101",
            PlayMode = 0,
            AryStageInfoes =
            [
                new()
                {
                    SongNo = 101,
                    Level = 2,
                    StageMode = 0,
                    PlayResult = 2,
                    PlayScore = 123456,
                    ScoreRate = 87,
                    IsFavorite = true,
                    IsRecent = true
                }
            ]
        };

        var result = await Ac15NormalPlayService.SaveAsync(
            baid: 1,
            playResult,
            Ac15EraProfiles.Blue,
            persistence,
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(persistence.PlayRows);
        Assert.Single(persistence.BestRows);
        Assert.Equal([(1u, 101u)], persistence.Favorites);
        Assert.Equal([(1u, 101u)], persistence.Recent);
        Assert.True(persistence.TrimRecentWasCalled);
        Assert.True(persistence.SaveWasCalled);
    }

    private sealed class FakePersistence(bool userExists) : IAc15NormalPlayPersistence
    {
        public bool SaveWasCalled { get; private set; }
        public bool TrimRecentWasCalled { get; private set; }
        public List<Ac15PlayRow> PlayRows { get; } = [];
        public List<Ac15BestRow> BestRows { get; } = [];
        public List<(uint Baid, uint SongNo)> Favorites { get; } = [];
        public List<(uint Baid, uint SongNo)> Recent { get; } = [];

        public ValueTask<bool> UserExistsAsync(uint baid, CancellationToken cancellationToken)
            => ValueTask.FromResult(userExists);

        public ValueTask<Ac15SaveSnapshot> GetOrCreateSaveAsync(uint baid, CancellationToken cancellationToken)
            => ValueTask.FromResult(new Ac15SaveSnapshot(baid));

        public ValueTask AddPlayRowAsync(Ac15PlayRow row, CancellationToken cancellationToken)
        {
            PlayRows.Add(row);
            return ValueTask.CompletedTask;
        }

        public ValueTask UpsertBestAsync(uint baid, Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken cancellationToken)
        {
            BestRows.Add(row);
            return ValueTask.CompletedTask;
        }

        public ValueTask SetFavoriteAsync(uint baid, uint songNo, bool isFavorite, int maxFavorites, CancellationToken cancellationToken)
        {
            if (isFavorite)
            {
                Favorites.Add((baid, songNo));
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask UpsertRecentAsync(uint baid, uint songNo, DateTime playTime, CancellationToken cancellationToken)
        {
            Recent.Add((baid, songNo));
            return ValueTask.CompletedTask;
        }

        public ValueTask TrimRecentAsync(uint baid, int maxRecent, CancellationToken cancellationToken)
        {
            TrimRecentWasCalled = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveWasCalled = true;
            return ValueTask.CompletedTask;
        }
    }
}
