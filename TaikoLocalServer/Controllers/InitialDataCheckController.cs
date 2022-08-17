using Swan.Extensions;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/initialdatacheck.php")]
public class InitialDataCheckController:ControllerBase
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
        
        var enabledArray = new byte[1000];
        Array.Fill(enabledArray, (byte)1);
        var response = new InitialdatacheckResponse
        {
            Result = 1,
            IsDanplay = false,
            IsAibattle = false,
            IsClose = false,
            SongIntroductionEndDatetime = (DateTime.Now + TimeSpan.FromDays(999)).ToUnixTimeMilliseconds().ToString(),
            DefaultSongFlg = GZipBytesUtil.GetGZipBytes(enabledArray)
        };
        /*response.AryMovieInfoes.Add(new InitialdatacheckResponse.MovieData
        {
            MovieId = 2,
            EnableDays = 9999
        });*/

        response.AryTelopDatas.Add(new InitialdatacheckResponse.InformationData
        {
            InfoId = 0,
            VerupNo = 1
        });

        return Ok(response);
    }

}