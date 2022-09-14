using System.Collections;
using TaikoLocalServer.Services.Interfaces;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r03/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseController<InitialDataCheckController>
{
    private readonly IGameDataService gameDataService;

    public InitialDataCheckController(IGameDataService gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Initial data check request: {Request}", request.Stringify());

        var enabledArray =
            FlagCalculator.GetBitArrayFromIds(gameDataService.GetMusicList(), Constants.MUSIC_ID_MAX, Logger);

        var danData = new List<InitialdatacheckResponse.InformationData>();
        for (var danId = Constants.MIN_DAN_ID; danId <= Constants.MAX_DAN_ID; danId++)
        {
            danData.Add(new InitialdatacheckResponse.InformationData
            {
                InfoId = (uint)danId,
                VerupNo = 1
            });
        }
        
        var introData = new List<InitialdatacheckResponse.InformationData>();
        for (var setId = 1; setId <= gameDataService.GetSongIntroDictionary().Count; setId++)
        {
            introData.Add(new InitialdatacheckResponse.InformationData
            {
                InfoId = (uint)setId,
                VerupNo = 1
            });
        }
        
        var response = new InitialdatacheckResponse
        {
            Result = 1,
            IsDanplay = true,
            IsAibattle = false,
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
            ArySongIntroductionDatas =
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
            AryEventfolderDatas =
            {
                new InitialdatacheckResponse.InformationData
                {
                    InfoId = 1,
                    VerupNo = 1
                }
            }
        };*/
            /*response.AryMovieInfoes.Add(new InitialdatacheckResponse.MovieData
            {
                MovieId = 2,
                EnableDays = 9999
            });*/
        };
        response.AryDanOdaiDatas.AddRange(danData);
        response.ArySongIntroductionDatas.AddRange(introData);
        return Ok(response);
    }

}