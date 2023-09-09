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

		var aryVerUp = new List<InitialdatacheckResponse.VerupNoData1>();

		var response = new InitialdatacheckResponse
		{
			Result = 1,
			DefaultSongFlg = enabledArray,
			AchievementSongBit = enabledArray,
			UraReleaseBit = uraReleaseBit,
			SongIntroductionEndDatetime = DateTime.Now.AddYears(10).ToString(Constants.DATE_TIME_FORMAT),
			// AryAiEventDatas =
			// {
			// 	new InitialdatacheckResponse.AiEventData
			// 	{
			// 		AiEventId = 18,
			// 		TokenId = 4
			// 	}
			// },
			AryVerupNoData1s =
			{
				
				new InitialdatacheckResponse.VerupNoData1
				{
					MasterType = 1,
					VerupNo = 0
				},
				new InitialdatacheckResponse.VerupNoData1
				{
					MasterType = 2,
					VerupNo = 0
				},
				new InitialdatacheckResponse.VerupNoData1
				{
					MasterType = 3,
					VerupNo = 0
				},
				new InitialdatacheckResponse.VerupNoData1
				{
					MasterType = 4,
					VerupNo = 0
				},
				new InitialdatacheckResponse.VerupNoData1
				{
					MasterType = 5,
					VerupNo = 0
				}
			},
			// AryVerupNoData1s =
			// {
			// 	new InitialdatacheckResponse.VerupNoData2
			// 	{
			// 		MasterType = 3,
			// 		AryInformationDatas =
			// 		{
			// 			new InitialdatacheckResponse.VerupNoData2.InformationData
			// 			{
			// 				InfoId = 1,
			// 				VerupNo = 2
			// 			}
			// 		}
			// 	}
			// }
		};
		
		var danData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
		for (var danId = Constants.MIN_DAN_ID; danId <= Constants.MAX_DAN_ID; danId++)
			danData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
			{
				InfoId = (uint)danId,
				VerupNo = 1
			});
		for (uint i = 0; i < 11; i++)
		{
			var verUp2 = new InitialdatacheckResponse.VerupNoData2
			{
				MasterType = i,
			};
			verUp2.AryInformationDatas.AddRange(danData);
			response.AryVerupNoData2s.Add(verUp2);
		}
		response.AryChassisFunctionIds = new uint[] { 3 };


		return Ok(response);
	}

}