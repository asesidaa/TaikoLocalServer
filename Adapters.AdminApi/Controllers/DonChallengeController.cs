using TaikoLocalServer.Application.Handlers;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DonChallengeController : BaseAdminController<DonChallengeController>
{
    [HttpGet("/api/{era}/[controller]/availability")]
    public async Task<ActionResult<DonChallengeAvailabilityResponse>> GetAvailability(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
        {
            return EraRoute.BadEra(era);
        }

        return Ok(await Mediator.Send(new GetDonChallengeAvailabilityQuery(gameEra)));
    }

    [HttpGet("/api/{era}/[controller]/{baid}")]
    public async Task<ActionResult<DonChallengeResponse>> GetDonChallenge(string era, uint baid)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
        {
            return EraRoute.BadEra(era);
        }

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
        {
            return forbid;
        }

        return Ok(await Mediator.Send(new GetDonChallengeQuery(gameEra, baid)));
    }
}
