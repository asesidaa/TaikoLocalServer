using MediatR;
using SharedProject.Models;
using SharedProject.Models.Responses;
using TaikoLocalServer.Filters;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ChallengeCompeteManageController(IChallengeCompeteService challengeCompeteService) : BaseController<ChallengeCompeteManageController>
{
    [HttpGet]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<List<ChallengeCompetition>>> GetAllChallengeCompete()
    {
        List<ChallengeCompeteDatum> datum = await challengeCompeteService.GetAllChallengeCompete();
        List<ChallengeCompetition> converted = new();
        foreach (var data in datum)
        {
            var challengeCompetition = Mappers.ChallengeCompeMappers.MapData(data);
            challengeCompetition = await challengeCompeteService.FillData(challengeCompetition);
            converted.Add(challengeCompetition);
        }

        return Ok(converted);
    }

    [HttpGet("queryPage")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<ChallengeCompetitionResponse>> GetChallengeCompePage([FromQuery] uint mode = 0, [FromQuery] uint baid = 0, [FromQuery] int inProgress = 0, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? searchTerm = null)
    {
        if (page < 1)
        {
            return BadRequest(new { Message = "Page number cannot be less than 1." });
        }

        if (limit > 200)
        {
            return BadRequest(new { Message = "Limit cannot be greater than 200." });
        }

        if (mode == 0)
        {
            return BadRequest(new { Message = "Invalid mode." });
        }

        ChallengeCompetitionResponse response = await challengeCompeteService.GetChallengeCompetePage((CompeteModeType)mode, baid, inProgress != 0, page, limit, searchTerm);

        return Ok(response);
    }

    [HttpGet("test/{mode}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public ActionResult<List<ChallengeCompeteDatum>> testCreateCompete(uint mode)
    {
        ChallengeCompeteCreateInfo info = new()
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
                    RandomType = (int)RandomType.Messy
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

    [HttpPost("createOfficialCompete")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> CreateCompete(ChallengeCompeteCreateInfo challengeCompeteInfo)
    {
        Logger.LogInformation("CreateOfficialCompete : {Request}", JsonFormatter.JsonSerialize(challengeCompeteInfo));

        await challengeCompeteService.CreateCompete(0, challengeCompeteInfo);

        return NoContent();
    }

    [HttpPost("{baid}/createCompete")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> CreateCompete(uint baid, ChallengeCompeteCreateInfo challengeCompeteInfo)
    {
        Logger.LogInformation("CreateCompete : {Request}", JsonFormatter.JsonSerialize(challengeCompeteInfo));
        await challengeCompeteService.CreateCompete(baid, challengeCompeteInfo);

        return NoContent();
    }

    [HttpPost("{baid}/createChallenge/{targetBaid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> CreateChallenge(uint baid, uint targetBaid, ChallengeCompeteCreateInfo challengeCompeteInfo)
    {
        Logger.LogInformation("CreateChallenge : {Request}", JsonFormatter.JsonSerialize(challengeCompeteInfo));
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