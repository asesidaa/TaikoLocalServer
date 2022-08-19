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

        var musicAttributeManager = MusicAttributeManager.Instance;
        
        var releaseSongArray = musicAttributeManager.Musics.ToArray();
        var uraSongArray = musicAttributeManager.MusicsWithUra.ToArray();
        
        var response = new UserDataResponse
        {
            Result = 1,
            ToneFlg = GZipBytesUtil.GetGZipBytes(new byte[1000]),
            TitleFlg = GZipBytesUtil.GetGZipBytes(new byte[1000]),
            ReleaseSongFlg = GZipBytesUtil.GetGZipBytes(releaseSongArray),
            UraReleaseSongFlg = GZipBytesUtil.GetGZipBytes(uraSongArray),
            DefaultOptionSetting = new byte[] {0b10001001, 0b00000000},
            SongRecentCnt = 0,
            IsVoiceOn = true,
            IsSkipOn = true,
        };
        
        

        return Ok(response);
    }
}