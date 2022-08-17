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

        var enabledArray = new byte[1000];
        Array.Fill(enabledArray, (byte)1);
        var response = new UserDataResponse
        {
            Result = 1,
            ToneFlg = GZipBytesUtil.GetGZipBytes(new byte[1000]),
            TitleFlg = GZipBytesUtil.GetGZipBytes(new byte[1000]),
            ReleaseSongFlg = GZipBytesUtil.GetGZipBytes(enabledArray),
            UraReleaseSongFlg = GZipBytesUtil.GetGZipBytes(enabledArray),
            DefaultOptionSetting = GZipBytesUtil.GetGZipBytes(new byte[1000]),
            SongRecentCnt = 0
        };
        
        

        return Ok(response);
    }
}