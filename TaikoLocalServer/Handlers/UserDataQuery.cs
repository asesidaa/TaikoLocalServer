using System.Buffers.Binary;
using GameDatabase.Context;
using TaikoLocalServer.Models.Application;
using Throw;

namespace TaikoLocalServer.Handlers;

public record UserDataQuery(uint Baid) : IRequest<CommonUserDataResponse>;

public class UserDataQueryHandler(TaikoDbContext context, IGameDataService gameDataService, ILogger<UserDataQueryHandler> logger) 
    : IRequestHandler<UserDataQuery, CommonUserDataResponse>
{

    public async Task<CommonUserDataResponse> Handle(UserDataQuery request, CancellationToken cancellationToken)
    {
        var userData = await context.UserData.FindAsync(request.Baid, cancellationToken);
        userData.ThrowIfNull($"User not found for Baid {request.Baid}!");
        
        var unlockedSongIdList = userData.UnlockedSongIdList;

        var musicList = gameDataService.GetMusicList();
        var lockedSongsList = gameDataService.GetLockedSongsList();
        lockedSongsList = lockedSongsList.Except(unlockedSongIdList).ToList();
        var enabledMusicList = musicList.Except(lockedSongsList);
        var releaseSongArray =
            FlagCalculator.GetBitArrayFromIds(enabledMusicList, Constants.MUSIC_ID_MAX, logger);

        var defaultSongWithUraList = gameDataService.GetMusicWithUraList();
        var enabledUraMusicList = defaultSongWithUraList.Except(lockedSongsList);
        var uraSongArray =
            FlagCalculator.GetBitArrayFromIds(enabledUraMusicList, Constants.MUSIC_ID_MAX, logger);

        if (userData.ToneFlgArray.Count == 0)
        {
            userData.ToneFlgArray = [0];
            context.UserData.Update(userData);
            await context.SaveChangesAsync(cancellationToken);
        }
        
        var toneArray = FlagCalculator.GetBitArrayFromIds(userData.ToneFlgArray, gameDataService.GetToneFlagArraySize(), logger);
        
        var titleArray = FlagCalculator.GetBitArrayFromIds(userData.TitleFlgArray, gameDataService.GetTitleFlagArraySize(), logger);

        var recentSongs = await context.SongPlayData
            .Where(datum => datum.Baid == request.Baid)
            .OrderByDescending(datum => datum.PlayTime)
            .ThenByDescending(datum => datum.SongNumber)
            .Select(datum => datum.SongId)
            .ToArrayAsync(cancellationToken);

        // Use custom implementation as distinctby cannot guarantee preserved element
        var recentSet = new OrderedSet<uint>();
        foreach (var id in recentSongs)
        {
            recentSet.Add(id);
            if (recentSet.Count == 10)
            {
                break;
            }
        }

        recentSongs = recentSet.ToArray();
        
        var defaultOptions = new byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(defaultOptions, userData.OptionSetting);

        var difficultySettingArray = JsonHelper.GetUIntArrayFromJson(userData.DifficultySettingArray, 3, logger, nameof(userData.DifficultySettingArray));
        for (int i = 0; i < 3; i++)
        {
            if (difficultySettingArray[i] >= 2)
            {
                difficultySettingArray[i] -= 1;
            }
        }

        var difficultyPlayedArray = JsonHelper.GetUIntArrayFromJson(userData.DifficultyPlayedArray, 3, logger, nameof(userData.DifficultyPlayedArray));

        var response = new CommonUserDataResponse
        {
            Result = 1,
            ToneFlg = toneArray,
            TitleFlg = titleArray,
            ReleaseSongFlg = releaseSongArray,
            UraReleaseSongFlg = uraSongArray,
            AryFavoriteSongNoes = userData.FavoriteSongsArray.ToArray(),
            AryRecentSongNoes = recentSongs,
            DefaultOptionSetting = defaultOptions,
            NotesPosition = userData.NotesPosition,
            IsVoiceOn = userData.IsVoiceOn,
            IsSkipOn = userData.IsSkipOn,
            DifficultySettingCourse = difficultySettingArray[0],
            DifficultySettingStar = difficultySettingArray[1],
            DifficultySettingSort = difficultySettingArray[2],
            DifficultyPlayedCourse = difficultyPlayedArray[0],
            DifficultyPlayedStar = difficultyPlayedArray[1],
            DifficultyPlayedSort = difficultyPlayedArray[2],
            SongRecentCnt = (uint)recentSongs.Length,
            IsChallengecompe = false,
            // TODO: Other fields
        };

        return response;
    }
}