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

        return Ok(datum);
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