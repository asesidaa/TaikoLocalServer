using TaikoLocalServer.Filters;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ChallengeCompeteManageController(IChallengeCompeteService challengeCompeteService) : BaseController<ChallengeCompeteManageController>
{
    [HttpGet]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public ActionResult<List<ChallengeCompeteDatum>> GetAllChallengeCompete()
    {
        List<ChallengeCompeteDatum> datum = challengeCompeteService.GetAllChallengeCompete();
        foreach (var data in datum)
        {
            foreach (var participant in data.Participants)
            {
                participant.ChallengeCompeteData = null;
            }
            foreach (var song in data.Songs)
            {
                song.ChallengeCompeteData = null;
                foreach (var bestScore in song.BestScores)
                {
                    bestScore.ChallengeCompeteSongData = null;
                }
            }
        }

        return Ok(datum);
    }

    [HttpGet("test/{mode}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public ActionResult<List<ChallengeCompeteDatum>> testCreateCompete(uint mode)
    {
        ChallengeCompeteInfo info = new()
        {
            Name = "测试数据",
            Desc = "测试数据描述",
            CompeteMode = (CompeteModeType)mode,
            MaxParticipant = 100,
            LastFor = 365,
            RequiredTitle = 0,
            ShareType = ShareType.EveryOne,
            CompeteTargetType = CompeteTargetType.EveryOne,
            challengeCompeteSongs = [
                new() {
                    SongId = 1,
                    Difficulty = Difficulty.Oni,
                    RandomType = RandomType.Messy
                },
                new() {
                    SongId = 2,
                    Difficulty = Difficulty.Oni,
                },
                new() {
                    SongId = 3,
                    Difficulty = Difficulty.Oni,
                },
            ]
        };
        challengeCompeteService.CreateCompete(1, info);

        return NoContent();
    }

    [HttpPost("{baid}/createCompete")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> CreateCompete(uint baid, ChallengeCompeteInfo challengeCompeteInfo)
    {
        await challengeCompeteService.CreateCompete(baid, challengeCompeteInfo);

        return NoContent();
    }

    [HttpPost("{baid}/createChallenge/{targetBaid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> CreateChallenge(uint baid, uint targetBaid, ChallengeCompeteInfo challengeCompeteInfo)
    {
        await challengeCompeteService.CreateChallenge(baid, targetBaid, challengeCompeteInfo);

        return NoContent();
    }

    [HttpGet("{baid}/joinCompete/{compId}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<bool>> JoinCompete(uint baid, uint compId)
    {
        bool result = await challengeCompeteService.ParticipateCompete(compId, baid);

        return Ok(result);
    }

    [HttpGet("{baid}/acceptChallenge/{compId}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<bool>> AcceptChallenge(uint baid, uint compId)
    {
        bool result = await challengeCompeteService.AnswerChallenge(compId, baid, true);

        return Ok(result);
    }

    [HttpGet("{baid}/rejectChallenge/{compId}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<bool>> RejectChallenge(uint baid, uint compId)
    {
        bool result = await challengeCompeteService.AnswerChallenge(compId, baid, false);

        return Ok(result);
    }

}