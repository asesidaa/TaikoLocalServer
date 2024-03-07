using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r08_ww/chassis/initialdatacheck_vaosv643.php")]
public class InitialDataCheckController : BaseController<InitialDataCheckController>
{
    private readonly IGameDataService gameDataService;

    private readonly ServerSettings settings;

    public InitialDataCheckController(IGameDataService gameDataService, IOptions<ServerSettings> settings)
    {
        this.gameDataService = gameDataService;
        this.settings = settings.Value;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Initial data check request: {Request}", request.Stringify());

        var songIdMax = settings.EnableMoreSongs ? Constants.MUSIC_ID_MAX_EXPANDED : Constants.MUSIC_ID_MAX;

        var musicList = gameDataService.GetMusicList();
        var lockedSongsList = gameDataService.GetLockedSongsList();

        var enabledArray =
            FlagCalculator.GetBitArrayFromIds(musicList, songIdMax, Logger);

        var defaultSongList = musicList.Except(lockedSongsList);
        var defaultSongFlg =
            FlagCalculator.GetBitArrayFromIds(defaultSongList, songIdMax, Logger);

        var defaultSongWithUraList = gameDataService.GetMusicWithUraList();
        var uraReleaseBit =
            FlagCalculator.GetBitArrayFromIds(defaultSongWithUraList, songIdMax, Logger);

        var response = new InitialdatacheckResponse
        {
            Result = 1,
            DefaultSongFlg = defaultSongFlg,
            AchievementSongBit = enabledArray,
            UraReleaseBit = uraReleaseBit,
            SongIntroductionEndDatetime = DateTime.Now.AddYears(10).ToString(Constants.DATE_TIME_FORMAT),
        };

        var movieDataDictionary = gameDataService.GetMovieDataDictionary();
        foreach (var movieData in movieDataDictionary) response.AryMovieInfoes.Add(movieData.Value);

        var verupNo1 = new uint[] { 2, 3, 4, 5, 6, 7, 8, 13, 15, 24, 25, 26, 27, 28, 29, 30, 31, 104 };
        var aryVerUp = verupNo1.Select(i => new InitialdatacheckResponse.VerupNoData1
        {
            MasterType = i,
            VerupNo = 1
        })
            .ToList();
        response.AryVerupNoData1s.AddRange(aryVerUp);

        var danData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
        var danDataDictionary = gameDataService.GetDanDataDictionary();
        foreach (var danId in danDataDictionary.Keys)
        {
            gameDataService.GetDanDataDictionary().TryGetValue(danId, out var odaiData);
            danData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
            {
                InfoId = danId,
                VerupNo = odaiData?.VerupNo ?? 1
            });
        }
        var verUp2Type101 = new InitialdatacheckResponse.VerupNoData2
        {
            MasterType = 101,
        };
        verUp2Type101.AryInformationDatas.AddRange(danData);
        response.AryVerupNoData2s.Add(verUp2Type101);

        var gaidenData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
        var gaidenDataDictionary = gameDataService.GetGaidenDataDictionary();
        foreach (var gaidenId in gaidenDataDictionary.Keys)
        {
            gaidenDataDictionary.TryGetValue(gaidenId, out var odaiData);
            gaidenData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
            {
                InfoId = gaidenId,
                VerupNo = odaiData?.VerupNo ?? 1
            });
        }

        var verUp2Type102 = new InitialdatacheckResponse.VerupNoData2
        {
            MasterType = 102,
        };
        verUp2Type102.AryInformationDatas.AddRange(gaidenData);
        response.AryVerupNoData2s.Add(verUp2Type102);

        var eventFolderData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
        var eventFolderDictionary = gameDataService.GetFolderDictionary();
        foreach (var folderId in eventFolderDictionary.Keys)
        {
            eventFolderDictionary.TryGetValue(folderId, out var folderData);
            eventFolderData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
            {
                InfoId = folderId,
                VerupNo = folderData?.VerupNo ?? 1
            });
        }
        var verUp2Type103 = new InitialdatacheckResponse.VerupNoData2
        {
            MasterType = 103,
        };
        verUp2Type103.AryInformationDatas.AddRange(eventFolderData);
        response.AryVerupNoData2s.Add(verUp2Type103);

        var songIntroData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
        var songIntroDictionary = gameDataService.GetSongIntroDictionary();
        foreach (var setId in songIntroDictionary.Select(item => item.Value.SetId))
        {
            songIntroDictionary.TryGetValue(setId, out var introData);
            songIntroData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
            {
                InfoId = setId,
                VerupNo = introData?.VerupNo ?? 1
            });
        }
        var verUp2Type105 = new InitialdatacheckResponse.VerupNoData2
        {
            MasterType = 105,
        };
        verUp2Type105.AryInformationDatas.AddRange(songIntroData);
        response.AryVerupNoData2s.Add(verUp2Type105);

        response.AryChassisFunctionIds = new uint[] { 1, 2, 3 };

        return Ok(response);
    }

}