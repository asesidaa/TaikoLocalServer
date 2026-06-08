using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial async ValueTask<CommonUserDataResponse> HandleYellow(
        UserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Yellow baid {request.Baid}.");
        var saveData = await context.GetOrCreateYellowSaveDataAsync(request.Baid, cancellationToken);
        var yellow = gameDataService.Yellow();
        var favorites = await context.YellowFavoriteSongs
            .Where(song => song.Baid == request.Baid)
            .Select(song => song.SongNo)
            .ToArrayAsync(cancellationToken);
        var recent = await context.YellowRecentSongs
            .Where(song => song.Baid == request.Baid)
            .OrderByDescending(song => song.LastPlayed)
            .Select(song => song.SongNo)
            .Take(Ac15EraProfiles.Yellow.Limits.MaxRecentSongs)
            .ToArrayAsync(cancellationToken);

        var snapshot = Ac15CatalogSnapshotFactory.FromYellow(yellow);
        var userdata = YellowAc15UserDataAdapter.CreateSnapshot(
            saveData,
            snapshot,
            favorites,
            recent,
            unlockedShopItems: []);
        var response = Ac15UserDataService.BuildResponse(userdata, Ac15EraProfiles.Yellow);
        response.IsDevilYellow = saveData.IsDevil;
        response.IsExplainYellow = saveData.IsExplain;
        return response;
    }
}
