namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r01/chassis/userdata.php")]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Red route probe userdata.php request: {@Request}", request);
        return Ok(new UserDataResponse
        {
            Result = 1,
            AryFavoriteSongNoes = [],
            AryRecentSongNoes = [],
            RecommendBestSongs = []
        });
    }
}
