namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getsongintroduction.php")]
[ApiController]
public class GetSongIntroductionController : BaseController<GetSongIntroductionController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetSongIntroduction([FromBody] GetSongIntroductionRequest request)
    {
        Logger.LogInformation("GetSongIntroduction request : {Request}", request.Stringify());

        var response = new GetSongIntroductionResponse
        {
            Result = 1
        };

        var manager = SongIntroductionDataManager.Instance;
        foreach (var setId in request.SetIds)
        {
            manager.IntroDataList.TryGetValue(setId, out var introData);
            if (introData is null)
            {
                Logger.LogWarning("Requested set id {Id} does not exist!", setId);
                continue;
            }

            response.ArySongIntroductionDatas.Add(introData);
        }

        return Ok(response);
    }
}