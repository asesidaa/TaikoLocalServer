using System.Text.Json;
using SharedProject.Entities;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r08_ww/chassis/addtokencount_7547j3o4.php")]
[ApiController]
public class AddTokenCountController : BaseController<AddTokenCountController>
{
    private readonly IUserDatumService userDatumService;

    public AddTokenCountController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> AddTokenCount([FromBody] AddTokenCountRequest request)
    {
        Logger.LogInformation("AddTokenCount request : {Request}", request.Stringify());

        var user = await userDatumService.GetFirstUserDatumOrNull(request.Baid);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");

        foreach (var addTokenCountData in request.AryAddTokenCountDatas)
        {
            var tokenId = addTokenCountData.TokenId;
            var addTokenCount = addTokenCountData.AddTokenCount;
            var token = user.Tokens.FirstOrDefault(t => t.Id == tokenId);
            if (token != null)
            {
                token.Count += addTokenCount;
            }
            else
            {
                user.Tokens.Add(new Token
                {
                    Id = (int)tokenId,
                    Count = addTokenCount
                });
            }
        }

        await userDatumService.UpdateUserDatum(user);

        var response = new AddTokenCountResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}