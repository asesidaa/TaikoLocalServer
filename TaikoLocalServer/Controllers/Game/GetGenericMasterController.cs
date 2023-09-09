using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r00_cn/chassis/getgenericmaster.php")]
public class GetGenericMasterController : BaseController<GetGenericMasterController>
{
	private readonly IGameDataService gameDataService;

	private readonly ServerSettings settings;

	[HttpPost]
	[Produces("application/protobuf")]
	public IActionResult GetGenericMaster([FromBody] GetGenericMasterRequest request)
	{
		Logger.LogInformation("GetGenericMasterRequest: {Request}", request.Stringify());

		var response = new GetGenericMasterResponse()
		{
			Result = 1,
			VerupNo = 2,
			// EnableIdBit = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }
			EnableIdBit = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
		};


		return Ok(response);
	}

}