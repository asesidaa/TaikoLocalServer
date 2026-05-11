using System.Buffers.Binary;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UserDataQuery(uint Baid) : IRequest<CommonUserDataResponse>;

public class UserDataQueryHandler(ITaikoDbContext context, IGameDataCatalog gameDataService, ILogger<UserDataQueryHandler> logger, IOptions<ServerSettings> settings) 
    : IRequestHandler<UserDataQuery, CommonUserDataResponse>
{

    private readonly ServerSettings settings = settings.Value;

    public async ValueTask<CommonUserDataResponse> Handle(UserDataQuery request, CancellationToken cancellationToken)
    {
        var userData = await context.UserData.FindAsync(request.Baid, cancellationToken);
        userData.ThrowIfNull($"User not found for Baid {request.Baid}!");
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(request.Baid, cancellationToken);
        
        var unlockedSongIdList = saveData.UnlockedSongIdList;
        var unlockedUraSongIdList = saveData.UnlockedUraSongIdList;

        var songIdMax = settings.EnableMoreSongs ? settings.MoreSongsSize : DomainConstants.MusicIdMax;

        var musicList = gameDataService.GetMusicList();
        var lockedSongsList = gameDataService.GetLockedSongsList().Except(unlockedSongIdList).ToList();
        var lockedUraSongsList = gameDataService.GetLockedUraSongsList().Except(unlockedUraSongIdList).ToList();
        var enabledMusicList = musicList.Except(lockedSongsList);
        var releaseSongArray =
            FlagCalculator.GetBitArrayFromIds(enabledMusicList, songIdMax, logger);

        var defaultSongWithUraList = gameDataService.GetMusicWithUraList();
        var enabledUraMusicList = defaultSongWithUraList.Except(lockedUraSongsList);
        var uraSongArray =
            FlagCalculator.GetBitArrayFromIds(enabledUraMusicList, songIdMax, logger);

        if (saveData.ToneFlgArray.Count == 0)
        {
            saveData.ToneFlgArray = [0];
            await context.SaveChangesAsync(cancellationToken);
        }
        
        //var toneArray = FlagCalculator.GetBitArrayFromIds(saveData.ToneFlgArray, gameDataService.GetToneFlagArraySize(), logger);
        var toneArray = FlagCalculator.GetBitArrayTrue(gameDataService.GetToneFlagArraySize());
        
        var titleArray = FlagCalculator.GetBitArrayFromIds(saveData.TitleFlgArray, gameDataService.GetTitleFlagArraySize(), logger);

        var recentSongs = await context.SongPlayDataNijiiro
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
        BinaryPrimitives.WriteInt16LittleEndian(defaultOptions, saveData.OptionSetting);

        uint[] difficultySettingArray = [saveData.DifficultySettingCourse, saveData.DifficultySettingStar, saveData.DifficultySettingSort];
        for (int i = 0; i < 3; i++)
        {
            if (difficultySettingArray[i] >= 2)
            {
                difficultySettingArray[i] -= 1;
            }
        }
        
        var response = new CommonUserDataResponse
        {
            Result = 1,
            ToneFlg = toneArray,
            TitleFlg = titleArray,
            ReleaseSongFlg = releaseSongArray,
            UraReleaseSongFlg = uraSongArray,
            AryFavoriteSongNoes = saveData.FavoriteSongsArray.ToArray(),
            AryRecentSongNoes = recentSongs,
            DefaultOptionSetting = defaultOptions,
            NotesPosition = saveData.NotesPosition,
            IsVoiceOn = saveData.IsVoiceOn,
            IsSkipOn = saveData.IsSkipOn,
            DifficultySettingCourse = difficultySettingArray[0],
            DifficultySettingStar = difficultySettingArray[1],
            DifficultySettingSort = difficultySettingArray[2],
            DifficultyPlayedCourse = saveData.DifficultyPlayedCourse,
            DifficultyPlayedStar = saveData.DifficultyPlayedStar,
            DifficultyPlayedSort = saveData.DifficultyPlayedSort,
            SongRecentCnt = (uint)recentSongs.Length,
            IsChallengecompe = false,
            // TODO: Other fields
        };

        return response;
    }
}
