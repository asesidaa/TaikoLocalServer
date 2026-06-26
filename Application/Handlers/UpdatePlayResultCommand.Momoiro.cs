using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private partial async ValueTask<uint> HandleMomoiro(
        UpdateAc15PlayResultCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Baid == 0)
        {
            return 1;
        }

        var user = await context.UserData.FindAsync([request.Baid], cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Game uploading a non existing Momoiro user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var normal = playResultData.Normal;
        IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            stages,
            Ac15EraProfiles.Momoiro.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Momoiro playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        var saveData = await context.GetOrCreateMomoiroSaveDataAsync(request.Baid, cancellationToken);
        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApplyDonPoints(
                saveData,
                playResultData.Profile,
                validStages,
                Ac15ProfileCounterUpdater.Momoiro,
                Ac15UnlockFlagAccess.Momoiro,
                Ac15EraProfiles.Momoiro.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Momoiro Don point totals for baid {Baid}", request.Baid);
            return 1;
        }

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            MomoiroNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Momoiro.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumMomoiro, SongBestDatumMomoiro, MomoiroFavoriteSongs, MomoiroRecentSongs> MomoiroNormalPlayTables()
        => new(
            context.SongPlayDataMomoiro,
            context.SongBestDataMomoiro,
            context.MomoiroFavoriteSongs,
            context.MomoiroRecentSongs,
            Ac15NormalPlayMapper.ToMomoiroSongPlayDatum,
            Ac15NormalPlayMapper.ToMomoiroSongBestDatum,
            CreateFavorite: CreateMomoiroFavoriteAsync);

    private async ValueTask<MomoiroFavoriteSongs> CreateMomoiroFavoriteAsync(
        uint baid,
        uint songNo,
        CancellationToken cancellationToken)
    {
        var persistedOrders = await context.MomoiroFavoriteSongs
            .Where(song => song.Baid == baid)
            .Select(song => song.DisplayOrder)
            .ToArrayAsync(cancellationToken);
        var trackedOrders = context.MomoiroFavoriteSongs.Local
            .Where(song => song.Baid == baid)
            .Select(song => song.DisplayOrder);
        var displayOrder = persistedOrders.Concat(trackedOrders).DefaultIfEmpty(-1).Max() + 1;

        return new MomoiroFavoriteSongs
        {
            Baid = baid,
            SongNo = songNo,
            DisplayOrder = displayOrder
        };
    }
}
