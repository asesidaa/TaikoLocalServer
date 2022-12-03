using GameDatabase.Entities;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/selfbest.php")]
[ApiController]
public class SelfBestController : BaseController<SelfBestController>
{
    private readonly IGameDataService gameDataService;
    private readonly ISongBestDatumService songBestDatumService;

    public SelfBestController(ISongBestDatumService songBestDatumService, IGameDataService gameDataService)
    {
        this.songBestDatumService = songBestDatumService;
        this.gameDataService = gameDataService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("SelfBest request : {Request}", request.Stringify());

        var response = new SelfBestResponse
        {
            Result = 1,
            Level = request.Level
        };

        var requestDifficulty = (Difficulty)request.Level;
        requestDifficulty.Throw().IfOutOfRange();

        var playerBestData = await songBestDatumService.GetAllSongBestData(request.Baid);
        playerBestData = playerBestData
            .Where(datum => datum.Difficulty == requestDifficulty ||
                            (datum.Difficulty == Difficulty.UraOni && requestDifficulty == Difficulty.Oni))
            .ToList();
        foreach (var songNo in request.ArySongNoes)
        {
            if (!gameDataService.GetMusicAttributes().ContainsKey(songNo))
            {
                Logger.LogWarning("Music no {No} is missing!", songNo);
                continue;
            }

            var selfBestData = GetSongSelfBestData(playerBestData, songNo);

            response.ArySelfbestScores.Add(selfBestData);
        }

        response.ArySelfbestScores.Sort((data, otherData) => data.SongNo.CompareTo(otherData.SongNo));

        return Ok(response);
    }

    private static SelfBestResponse.SelfBestData GetSongSelfBestData(IEnumerable<SongBestDatum> playerBestData,
        uint songNo)
    {
        var songBestDatum = playerBestData.Where(datum => datum.SongId == songNo);

        var selfBestData = new SelfBestResponse.SelfBestData
        {
            SongNo = songNo
        };

        foreach (var datum in songBestDatum)
        {
            if (datum.Difficulty == Difficulty.UraOni)
            {
                selfBestData.UraBestScore = datum.BestScore;
                selfBestData.UraBestScoreRate = datum.BestRate;
                continue;
            }

            selfBestData.SelfBestScore = datum.BestScore;
            selfBestData.SelfBestScoreRate = datum.BestRate;
        }

        return selfBestData;
    }
}