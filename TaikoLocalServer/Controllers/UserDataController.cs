using Microsoft.AspNetCore.Http;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/userdata.php")]
[ApiController]
public class UserDataController : ControllerBase
{
    private readonly ILogger<UserDataController> logger;
    public UserDataController(ILogger<UserDataController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetUserData([FromBody] UserDataRequest request)
    {
        logger.LogInformation("UserData request : {Request}", request.Stringify());

        var response = new UserDataResponse
        {
            Result = 1,
            ToneFlg = GZipBytesUtil.GetEmptyJsonGZipBytes(),
            TitleFlg = GZipBytesUtil.GetEmptyJsonGZipBytes(),
            ReleaseSongFlg = GZipBytesUtil.GetEmptyJsonGZipBytes(),
            UraReleaseSongFlg = GZipBytesUtil.GetEmptyJsonGZipBytes(),
            DefaultOptionSetting = GZipBytesUtil.GetEmptyJsonGZipBytes(),
            DispScoreType = 0,
            DispLevelChassis = 0,
            DispLevelSelf = 0,
            IsDispTojiruOn = true,
            NotesPosition = 0,
            IsVoiceOn = true,
            IsChallengecompe = false,
            IsSkipOn = true,
            DifficultyPlayedCourse = 0,
            DifficultyPlayedStar = 0,
            TotalCreditCnt = 1,
            SongRecentCnt = 0
        };

        return Ok(response);
    }
}