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
		
		var lockedSongsList = gameDataService.GetLockedSongsList();
		var enabledMusicList = gameDataService.GetMusicList().Except(lockedSongsList);
		var enabledUraMusicList = gameDataService.GetMusicWithUraList().Except(lockedSongsList);

		var enabledArray =
			FlagCalculator.GetBitArrayFromIds(enabledMusicList, songIdMax, Logger);
		var uraReleaseBit = FlagCalculator.GetBitArrayFromIds(enabledUraMusicList, songIdMax, Logger);

		var verupNo1 = new uint[] { 104 };
		var aryVerUp = verupNo1.Select(i => new InitialdatacheckResponse.VerupNoData1
			{
				MasterType = i,
				VerupNo = 1
			})
			.ToList();

		var response = new InitialdatacheckResponse
		{
			Result = 1,
			DefaultSongFlg = enabledArray,
			AchievementSongBit = enabledArray,
			UraReleaseBit = uraReleaseBit,
			SongIntroductionEndDatetime = DateTime.Now.AddYears(10).ToString(Constants.DATE_TIME_FORMAT),
		};
		response.AryVerupNoData1s.AddRange(aryVerUp);
		
		var danData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
		for (var danId = Constants.MIN_DAN_ID; danId <= 18; danId++)
		{
			danData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
			{
				InfoId = (uint)danId,
				VerupNo = 1
			});
		}
		var verupNo2 = new uint[] { 101 };
		foreach (var i in verupNo2)
		{
			var verUp2 = new InitialdatacheckResponse.VerupNoData2
			{
				MasterType = i,
			};
			verUp2.AryInformationDatas.AddRange(danData);
			response.AryVerupNoData2s.Add(verUp2);
		}
		response.AryChassisFunctionIds = new uint[] {1,2,3};


		return Ok(response);
	}

}