using Swan.Extensions;
using TaikoLocalServer.Common;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/initialdatacheck.php")]
public class InitialDataCheckController : ControllerBase
{
    private readonly ILogger<InitialDataCheckController> logger;

    public InitialDataCheckController(ILogger<InitialDataCheckController> logger)
    {
        this.logger = logger;
    }


    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        logger.LogInformation("InitialDataCheck request: {Request}", request.Stringify());

        var enabledArray = new uint[] {1, 2, 7, 11, 14, 17};
        var response = new InitialdatacheckResponse
        {
            Result = 1,
            IsDanplay = false,
            IsAibattle = false,
            IsClose = false,
            SongIntroductionEndDatetime = (DateTime.Now + TimeSpan.FromDays(999)).ToString(Constants.DATE_TIME_FORMAT),
            DefaultSongFlg = GZipBytesUtil.GetGZipBytes(enabledArray),
            AryTelopDatas =
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
            AryShopFolderDatas =
            {
                new InitialdatacheckResponse.InformationData
                {
                    InfoId = 1,
                    VerupNo = 1
                }
            },
            AryDanOdaiDatas =
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
        };
        /*response.AryMovieInfoes.Add(new InitialdatacheckResponse.MovieData
        {
            MovieId = 2,
            EnableDays = 9999
        });*/
        return Ok(response);
    }

}