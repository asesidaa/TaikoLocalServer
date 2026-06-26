using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<Ac15UserDataResponse> HandleMomoiro(
        Ac15UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Momoiro baid {request.Baid}.");
        var saveData = await context.GetOrCreateMomoiroSaveDataAsync(request.Baid, cancellationToken);
        var momoiro = gameDataService.Momoiro();
        var limits = Ac15EraProfiles.Momoiro.Limits;

        var favorites = await context.MomoiroFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .OrderBy(song => song.DisplayOrder)
            .ThenBy(song => song.SongNo)
            .Select(song => song.SongNo)
            .Take(limits.MaxFavoriteSongs)
            .ToArrayAsync(cancellationToken);
        var recent = await context.MomoiroRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(limits.MaxRecentSongs)
            .ToArrayAsync(cancellationToken);
        var bestRows = await context.SongBestDataMomoiro
            .Where(row => row.Baid == request.Baid)
            .Select(row => new Ac15BestRow(
                row.SongId,
                row.Difficulty,
                row.IsShin,
                row.BestScore,
                row.BestRate,
                row.BestCrown))
            .ToArrayAsync(cancellationToken);

        var snapshot = Ac15CatalogSnapshotFactory.FromMomoiro(momoiro);
        var userdata = MomoiroAc15UserDataAdapter.CreateSnapshot(saveData, snapshot, favorites, recent);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Momoiro);
        var crownBytes = Ac15CrownService.BuildCatalogOrderBody(
            bestRows,
            momoiro.MusicInfoFileOrder.Select(song => song.SongNo),
            limits);

        return response with
        {
            ModeFlags = new Ac15UserDataModeFlags(saveData.IsDevil, saveData.IsExplain),
            Tutorial = new Ac15UserDataTutorial(null, saveData.DifficultyTutorialFlg),
            Reward = new Ac15UserDataReward(
                saveData.TotalGetDonpoint,
                saveData.TotalUseDonpoint,
                saveData.RewardProgress),
            HashCrownFlg = crownBytes
        };
    }
}
