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
			SongIntroductionEndDatetime = DateTime.Now.AddYears(10).ToString(Constants.DATE_TIME_FORMAT),
		};
		
		var movieDataDictionary = gameDataService.GetMovieDataDictionary();
		foreach (var movieData in movieDataDictionary) response.AryMovieInfoes.Add(movieData.Value);

		var verupNo1 = new uint[] { 2, 3, 4, 5, 6, 7, 8, 13, 15, 24, 25, 26, 27, 28, 29, 30, 31, 104 };
		var aryVerUp = verupNo1.Select(i => new InitialdatacheckResponse.VerupNoData1
			{
				MasterType = i,
				VerupNo = 1
			})
			.ToList();
		response.AryVerupNoData1s.AddRange(aryVerUp);
		
		var danData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
		for (var danId = Constants.MIN_DAN_ID; danId <= Constants.MAX_DAN_ID; danId++)
		{
			danData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
			{
				InfoId = (uint)danId,
				VerupNo = 1
			});
		}
		var verUp2Type101 = new InitialdatacheckResponse.VerupNoData2
		{
			MasterType = 101,
		};
		verUp2Type101.AryInformationDatas.AddRange(danData);
		response.AryVerupNoData2s.Add(verUp2Type101);
		
		var verUp2Type102 = new InitialdatacheckResponse.VerupNoData2
		{
			MasterType = 102,
		};
		verUp2Type102.AryInformationDatas.AddRange(danData);
		response.AryVerupNoData2s.Add(verUp2Type102);
		
		var eventFolderData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
		foreach (var folderId in Constants.EVENT_FOLDER_IDS)
		{
			eventFolderData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
			{
				InfoId = (uint)folderId,
				VerupNo = 1
			});
		}
		var verUp2Type103 = new InitialdatacheckResponse.VerupNoData2
		{
			MasterType = 103,
		};
		verUp2Type103.AryInformationDatas.AddRange(eventFolderData);
		response.AryVerupNoData2s.Add(verUp2Type103);

		var songIntroData = new List<InitialdatacheckResponse.VerupNoData2.InformationData>();
		var verUp2Type105 = new InitialdatacheckResponse.VerupNoData2
		{
			MasterType = 105,
		};
		for (var setId = 1; setId <= gameDataService.GetSongIntroDictionary().Count; setId++)
		{
			songIntroData.Add(new InitialdatacheckResponse.VerupNoData2.InformationData
			{
				InfoId = (uint)setId,
				VerupNo = 1
			});
		}
		verUp2Type105.AryInformationDatas.AddRange(songIntroData);
		response.AryVerupNoData2s.Add(verUp2Type105);
		
		response.AryChassisFunctionIds = new uint[] {1,2,3};

		return Ok(response);
	}

}