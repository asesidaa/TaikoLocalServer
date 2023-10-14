using Microsoft.Extensions.Options;
using System.Buffers.Binary;
using System.Text.Json;
using TaikoLocalServer.Settings;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r00_cn/chassis/userdata.php")]
[ApiController]
public class UserDataController : BaseController<UserDataController>
{
    private readonly IUserDatumService userDatumService;

    private readonly ISongPlayDatumService songPlayDatumService;

    private readonly IGameDataService gameDataService;

    private readonly ServerSettings settings;

    public UserDataController(IUserDatumService userDatumService, ISongPlayDatumService songPlayDatumService,
        IGameDataService gameDataService, IOptions<ServerSettings> settings)
    {
        this.userDatumService = userDatumService;
        this.songPlayDatumService = songPlayDatumService;
        this.gameDataService = gameDataService;
        this.settings = settings.Value;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetUserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("UserData request : {Request}", request.Stringify());

        var songIdMax = settings.EnableMoreSongs ? Constants.MUSIC_ID_MAX_EXPANDED : Constants.MUSIC_ID_MAX;

        var userData = await userDatumService.GetFirstUserDatumOrDefault(request.Baid);
        
        var unlockedSongIdList = new List<uint>();
        try
        {
            unlockedSongIdList = !string.IsNullOrEmpty(userData.UnlockedSongIdList)
                ? JsonSerializer.Deserialize<List<uint>>(userData.UnlockedSongIdList)
                : new List<uint>();
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing UnlockedSongIdList data for user with baid {Request} failed!", request.Baid);
        }

        unlockedSongIdList.ThrowIfNull("UnlockedSongIdList should never be null");
        
        var musicList = gameDataService.GetMusicList();
        var lockedSongsList = gameDataService.GetLockedSongsList();
        lockedSongsList = lockedSongsList.Except(unlockedSongIdList).ToList();
        var enabledMusicList = musicList.Except(lockedSongsList);
        var releaseSongArray =
            FlagCalculator.GetBitArrayFromIds(enabledMusicList, songIdMax, Logger);

        var defaultSongWithUraList = gameDataService.GetMusicWithUraList();
        var enabledUraMusicList = defaultSongWithUraList.Except(lockedSongsList);
        var uraSongArray =
            FlagCalculator.GetBitArrayFromIds(enabledUraMusicList, songIdMax, Logger);

        var toneFlg = Array.Empty<uint>();
        try
        {
            toneFlg = JsonSerializer.Deserialize<uint[]>(userData.ToneFlgArray);
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing tone flg json data failed");
        }

        // The only way to get a null is provide string "null" as input,
        // which means database content need to be fixed, so better throw
        toneFlg.ThrowIfNull("Tone flg should never be null!");
        
        // If toneFlg is empty, add 0 to it
        if (toneFlg.Length == 0)
        {
            toneFlg = new uint[] { 0 };
            userData.ToneFlgArray = JsonSerializer.Serialize(toneFlg);
            await userDatumService.UpdateUserDatum(userData);
        }

        var toneArray = FlagCalculator.GetBitArrayFromIds(toneFlg, gameDataService.GetToneFlagArraySize(), Logger);

        var titleFlg = Array.Empty<uint>();
        try
        {
            titleFlg = JsonSerializer.Deserialize<uint[]>(userData.TitleFlgArray);
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing title flg json data failed");
        }

        // The only way to get a null is provide string "null" as input,
        // which means database content need to be fixed, so better throw
        titleFlg.ThrowIfNull("Title flg should never be null!");

        var titleArray = FlagCalculator.GetBitArrayFromIds(titleFlg, gameDataService.GetTitleFlagArraySize(), Logger);

        var recentSongs = (await songPlayDatumService.GetSongPlayDatumByBaid(request.Baid))
            .AsEnumerable()
            .OrderByDescending(datum => datum.PlayTime)
            .ThenByDescending(datum => datum.SongNumber)
            .Select(datum => datum.SongId)
            .ToArray();

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

        var favoriteSongs = Array.Empty<uint>();
        try
        {
            favoriteSongs = JsonSerializer.Deserialize<uint[]>(userData.FavoriteSongsArray);
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing favorite songs json data failed");
        }

        // The only way to get a null is provide string "null" as input,
        // which means database content need to be fixed, so better throw
        favoriteSongs.ThrowIfNull("Favorite song should never be null!");

        var defaultOptions = new byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(defaultOptions, userData.OptionSetting);

        var difficultySettingArray = JsonHelper.GetUIntArrayFromJson(userData.DifficultySettingArray, 3, Logger, nameof(userData.DifficultySettingArray));
        for (int i = 0; i < 3; i++)
        {
            if (difficultySettingArray[i] >= 2)
            {
                difficultySettingArray[i] -= 1;
            }
        }

        var difficultyPlayedArray = JsonHelper.GetUIntArrayFromJson(userData.DifficultyPlayedArray, 3, Logger, nameof(userData.DifficultyPlayedArray));

        var response = new UserDataResponse
        {
            Result = 1,
            ToneFlg = toneArray,
            TitleFlg = titleArray,
            ReleaseSongFlg = releaseSongArray,
            UraReleaseSongFlg = uraSongArray,
            AryFavoriteSongNoes = favoriteSongs,
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
            IsChallengecompe = false,
            SongRecentCnt = (uint)recentSongs.Length
        };

        return Ok(response);
    }
}