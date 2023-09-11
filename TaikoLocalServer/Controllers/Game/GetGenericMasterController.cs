using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r00_cn/chassis/getgenericmaster.php")]
public class GetGenericMasterController : BaseController<GetGenericMasterController>
{
	[HttpPost]
	[Produces("application/protobuf")]
	public IActionResult GetGenericMaster([FromBody] GetGenericMasterRequest request)
	{
		Logger.LogInformation("GetGenericMasterRequest: {Request}", request.Stringify());

		var response = new GetGenericMasterResponse
		{
			Result = 1,
			VerupNo = 2,
			EnableIdBit = FlagCalculator.GetBitArrayTrue(20)
		};


		return Ok(response);
	}

}