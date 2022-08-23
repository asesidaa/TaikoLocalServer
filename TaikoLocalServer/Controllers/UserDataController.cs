using System.Collections;
using System.Runtime.InteropServices;
using TaikoLocalServer.Common;
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

        var releaseSongArray = new byte[Constants.MUSIC_FLAG_ARRAY_SIZE];
        var test = new BitArray(Constants.MUSIC_ID_MAX);
        foreach (var music in musicAttributeManager.Musics)
        {
            test.Set((int)music, true);
        }
        test.CopyTo(releaseSongArray, 0); 
        
        var uraSongArray = new byte[Constants.MUSIC_FLAG_ARRAY_SIZE];
        test.SetAll(false);
        foreach (var music in musicAttributeManager.MusicsWithUra)
        {
            test.Set((int)music, true);
        }
        test.CopyTo(uraSongArray, 0);
        
        var response = new UserDataResponse
        {
            Result = 1,
            ToneFlg = new byte[] {9},
            // TitleFlg = GZipBytesUtil.GetGZipBytes(new byte[100]),
            ReleaseSongFlg = releaseSongArray,
            UraReleaseSongFlg = uraSongArray,
            DefaultOptionSetting = new byte[] {0b10001001, 0b00000000},
            SongRecentCnt = 0,
            IsVoiceOn = true,
            IsSkipOn = true
        };
        
        

        return Ok(response);
    }
}