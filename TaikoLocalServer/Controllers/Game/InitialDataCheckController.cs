using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r03/chassis/initialdatacheck.php")]
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
        var enabledArray =
            FlagCalculator.GetBitArrayFromIds(gameDataService.GetMusicList(), songIdMax, Logger);

        var danData = new List<InitialdatacheckResponse.InformationData>();
        for (var danId = Constants.MIN_DAN_ID; danId <= Constants.MAX_DAN_ID; danId++)
            danData.Add(new InitialdatacheckResponse.InformationData
            {
                InfoId = (uint)danId,
                VerupNo = 1
            });

        var introData = new List<InitialdatacheckResponse.InformationData>();
        for (var setId = 1; setId <= gameDataService.GetSongIntroDictionary().Count; setId++)
            introData.Add(new InitialdatacheckResponse.InformationData
            {
                InfoId = (uint)setId,
                VerupNo = 1
            });

        var eventFolderData = Constants.EVENT_FOLDER_IDS.Select(folderId => new InitialdatacheckResponse.InformationData
            { InfoId = (uint)folderId, VerupNo = 0 }).ToList();


        var response = new InitialdatacheckResponse
        {
            Result = 1,
            IsDanplay = true,
            IsAibattle = true,
            IsClose = false,
            DefaultSongFlg = enabledArray,
            AchievementSongBit = enabledArray,
            SongIntroductionEndDatetime = DateTime.Now.AddYears(10).ToString(Constants.DATE_TIME_FORMAT),
            AryShopFolderDatas =
            {
                new InitialdatacheckResponse.InformationData
                {
                    InfoId = 1,
                    VerupNo = 2
                }
            }
            /*AryTelopDatas =
            {
                new InitialdatacheckResponse.InformationData
                {
                    InfoId = 1,
                    VerupNo = 1
                }
            },
            AryDanextraOdaiDatas =
            {
                new InitialdatacheckResponse.InformationData
                {
                    InfoId = 1,
                    VerupNo = 1
                }
            },
            AryAiEventDatas =
            {
                new InitialdatacheckResponse.AiEventData
                {
                    AiEventId = 1,
                    TokenId = 1
                }
            },
            AryMovieInfos = 
            {
                new InitialdatacheckResponse.MovieData
                {
                    MovieId = 2,
                    EnableDays = 9999
                }
            }*/
        };

        response.AryDanOdaiDatas.AddRange(danData);
        response.ArySongIntroductionDatas.AddRange(introData);
        response.AryEventfolderDatas.AddRange(eventFolderData);
        return Ok(response);
    }
}