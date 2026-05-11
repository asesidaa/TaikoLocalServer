namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/getghostdata.php")]
public class GetGhostDataController : BaseProtocolController<GetGhostDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetGhostData([FromBody] GetghostdataRequest request)
    {
        Logger.LogInformation("Green GetGhostData request: {Request}", request.Stringify());
        return Ok(new GetghostdataResponse
        {
            Result = 1,
            ReleaseInfoFlag = [],
            PlayedSongFlag = [],
            TotalWinnings = 0,
            ghost_perf_data = new GetghostdataResponse.GhostPerfData(),
            GhostRecordData = new GetghostdataResponse.GhostRankData()
        });
    }
}
