using System.Text.Json;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r08_ww/chassis/songpurchase_wm2fh5bl.php")]
[ApiController]
public class SongPurchaseController : BaseController<SongPurchaseController>
{
    private readonly IUserDatumService userDatumService;

    public SongPurchaseController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SongPurchase([FromBody] SongPurchaseRequest request)
    {
        Logger.LogInformation("SongPurchase request : {Request}", request.Stringify());

        var user = await userDatumService.GetFirstUserDatumOrNull(request.Baid);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");

        Logger.LogInformation("Original UnlockedSongIdList: {UnlockedSongIdList}", user.UnlockedSongIdList);

        var unlockedSongIdList = user.UnlockedSongIdList.ToList();

        var token = user.Tokens.FirstOrDefault(t => t.Id == request.TokenId);
        if (token is not null && token.Count >= request.Price)
        {
            token.Count -= (int)request.Price;
        }
        else
        {
            Logger.LogError("User with baid {Baid} does not have enough tokens to purchase song with id {SongNo}!", request.Baid, request.SongNo);
            return Ok(new SongPurchaseResponse { Result = 0 });
        }
        
        if (!unlockedSongIdList.Contains(request.SongNo)) unlockedSongIdList.Add(request.SongNo);

        user.UnlockedSongIdList = unlockedSongIdList.ToArray();

        Logger.LogInformation("Updated UnlockedSongIdList: {UnlockedSongIdList}", user.UnlockedSongIdList);

        await userDatumService.UpdateUserDatum(user);

        var response = new SongPurchaseResponse
        {
            Result = 1,
            TokenCount = token.Count
        };

        return Ok(response);
    }
}