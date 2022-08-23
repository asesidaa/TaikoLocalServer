using TaikoLocalServer.Common;
using TaikoLocalServer.Common.Enums;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/baidcheck.php")]
public class BaidController:ControllerBase
{
    private readonly ILogger<BaidController> logger;

    public BaidController(ILogger<BaidController> logger)
    {
        this.logger = logger;
    }


    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] BAIDRequest request)
    {
        logger.LogInformation("Baid request: {Request}", request.Stringify());
        var response = new BAIDResponse
        {
            Result = 1,
            PlayerType = 0,
            ComSvrResult = 1,
            MbId = 1,
            Baid = 348230000,
            AccessCode = request.AccessCode,
            IsPublish = true,
            CardOwnNum = 1,
            RegCountryId = "JPN",
            PurposeId = 1,
            RegionId = 1,
            MydonName = "test",
            Title = "test",
            TitleplateId = 1,
            ColorFace = 1,
            ColorBody = 1,
            ColorLimb = 1,
            AryCostumedata = new BAIDResponse.CostumeData
            {
                Costume1 = 1,
                Costume2 = 1,
                Costume3 = 1,
                Costume4 = 1,
                Costume5 = 1
            },
            CostumeFlg1 = new byte[10],
            CostumeFlg2 = new byte[10],
            CostumeFlg3 = new byte[10],
            CostumeFlg4 = new byte[10],
            CostumeFlg5 = new byte[10],
            LastPlayDatetime = (DateTime.Now - TimeSpan.FromDays(1)).ToString(Constants.DATE_TIME_FORMAT),
            IsDispDanOn = true,
            GotDanMax = 1,
            GotDanFlg = new byte[20],
            GotDanextraFlg = new byte[20],
            DefaultToneSetting = 0,
            GenericInfoFlg = new byte[10],
            AryCrownCounts = new uint[] {0,0,0},
            AryScoreRankCounts = new uint[]
            {
                0,0,0,0,0,0,0
            },
            IsDispAchievementOn = true,
            DispAchievementType = (uint)Difficulty.Oni,
            IsDispAchievementTypeSet = false,
            LastPlayMode = 0,
            IsDispSouuchiOn = true,
            AiRank = 0,
            AiTotalWin = 0,
            Accesstoken = "123456",
            ContentInfo = GZipBytesUtil.GetGZipBytes(new byte[10])
        };

        return Ok(response);
    }

}