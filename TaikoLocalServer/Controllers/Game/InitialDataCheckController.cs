using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r00_cn/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseController<InitialDataCheckController>
{
	private readonly IGameDataService gameDataService;

	private readonly ServerSettings settings;

	public InitialDataCheckController(IGameDataService gameDataService, IOptions<ServerSettings> settings)
	{
		this.gameDataService = gameDataService;
		this.settings = settings.Value;
	}

	[HttpPost]
	[Produces("application/protobuf")]
	public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
	{
		Logger.LogInformation("Initial data check request: {Request}", request.Stringify());

		var songIdMax = settings.EnableMoreSongs ? Constants.MUSIC_ID_MAX_EXPANDED : Constants.MUSIC_ID_MAX;
		var enabledArray =
			FlagCalculator.GetBitArrayFromIds(gameDataService.GetMusicList(), songIdMax, Logger);

		var defaultSongWithUraList = gameDataService.GetMusicWithUraList();
		var uraReleaseBit =
			FlagCalculator.GetBitArrayFromIds(defaultSongWithUraList, songIdMax, Logger);

		var response = new InitialdatacheckResponse
		{
			Result = 1,
			DefaultSongFlg = enabledArray,
			AchievementSongBit = enabledArray,
			UraReleaseBit = uraReleaseBit,
			SongIntroductionEndDatetime = DateTime.Now.AddYears(10).ToString(Constants.DATE_TIME_FORMAT)
		};

		return Ok(response);
	}

}