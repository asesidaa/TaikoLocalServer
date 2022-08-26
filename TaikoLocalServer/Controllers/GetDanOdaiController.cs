namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getdanodai.php")]
[ApiController]
public class GetDanOdaiController : ControllerBase
{
    private readonly ILogger<GetDanOdaiController> logger;
    public GetDanOdaiController(ILogger<GetDanOdaiController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetDanOdai([FromBody] GetDanOdaiRequest request)
    {
        logger.LogInformation("GetDanOdai request : {Request}", request.Stringify());

        var response = new GetDanOdaiResponse
        {
            Result = 1
        };

        foreach (var danId in request.DanIds)
        {
            response.AryOdaiDatas.Add(new GetDanOdaiResponse.OdaiData
            {
                DanId = danId,
                Title = "5kyuu",
                VerupNo = 1,
                AryOdaiSongs = { new GetDanOdaiResponse.OdaiData.OdaiSong
                {
                    IsHiddenSongName = false,
                    Level = 1,
                    SongNo = 956
                },new GetDanOdaiResponse.OdaiData.OdaiSong
                {
                    IsHiddenSongName = false,
                    Level = 1,
                    SongNo = 839
                },new GetDanOdaiResponse.OdaiData.OdaiSong
                {
                    IsHiddenSongName = false,
                    Level = 1,
                    SongNo = 937
                } },
                AryOdaiBorders = { new GetDanOdaiResponse.OdaiData.OdaiBorder
                {
                    OdaiType = 1,
                    BorderType = 1,
                    RedBorder1 = 1,
                    RedBorder2 = 5,
                    RedBorder3 = 8,
                    RedBorderTotal = 10,
                    GoldBorder1 = 1,
                    GoldBorder2 = 5,
                    GoldBorder3 = 8,
                    GoldBorderTotal = 10
                } ,new GetDanOdaiResponse.OdaiData.OdaiBorder
                {
                    OdaiType = 2,
                    BorderType = 1,
                    RedBorder1 = 1,
                    RedBorder2 = 5,
                    RedBorder3 = 8,
                    RedBorderTotal = 10,
                    GoldBorder1 = 1,
                    GoldBorder2 = 5,
                    GoldBorder3 = 8,
                    GoldBorderTotal = 10
                } ,new GetDanOdaiResponse.OdaiData.OdaiBorder
                {
                    OdaiType = 3,
                    BorderType = 1,
                    RedBorder1 = 1,
                    RedBorder2 = 5,
                    RedBorder3 = 8,
                    RedBorderTotal = 10,
                    GoldBorder1 = 1,
                    GoldBorder2 = 5,
                    GoldBorder3 = 8,
                    GoldBorderTotal = 10
                } ,new GetDanOdaiResponse.OdaiData.OdaiBorder
                {
                    OdaiType = 4,
                    BorderType = 1,
                    RedBorder1 = 1,
                    RedBorder2 = 5,
                    RedBorder3 = 8,
                    RedBorderTotal = 10,
                    GoldBorder1 = 1,
                    GoldBorder2 = 5,
                    GoldBorder3 = 8,
                    GoldBorderTotal = 10
                } }
            });
        }

        return Ok(response);
    }
}