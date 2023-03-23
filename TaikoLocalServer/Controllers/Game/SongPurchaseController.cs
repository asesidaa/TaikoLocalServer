using System.Text.Json;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/songpurchase.php")]
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

        var tokenCountDict = new Dictionary<uint, int>();
        try
        {
            tokenCountDict = !string.IsNullOrEmpty(user.TokenCountDict)
                ? JsonSerializer.Deserialize<Dictionary<uint, int>>(user.TokenCountDict)
                : new Dictionary<uint, int>();
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing TokenCountDict data for user with baid {Request} failed!", request.Baid);
        }

        tokenCountDict.ThrowIfNull("TokenCountDict should never be null");
        
        Logger.LogInformation("Original UnlockedSongIdList: {UnlockedSongIdList}", user.UnlockedSongIdList);

        var unlockedSongIdList = new List<uint>();
        try
        {
            unlockedSongIdList = !string.IsNullOrEmpty(user.UnlockedSongIdList)
                ? JsonSerializer.Deserialize<List<uint>>(user.UnlockedSongIdList)
                : new List<uint>();
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing UnlockedSongIdList data for user with baid {Request} failed!", request.Baid);
        }

        unlockedSongIdList.ThrowIfNull("UnlockedSongIdList should never be null");

        if (tokenCountDict.ContainsKey(request.TokenId)) tokenCountDict[request.TokenId] -= (int)request.Price;

        if (!unlockedSongIdList.Contains(request.SongNo)) unlockedSongIdList.Add(request.SongNo);

        user.TokenCountDict = JsonSerializer.Serialize(tokenCountDict);
        user.UnlockedSongIdList = JsonSerializer.Serialize(unlockedSongIdList);
        
        Logger.LogInformation("Updated UnlockedSongIdList: {UnlockedSongIdList}", user.UnlockedSongIdList);
        
        await userDatumService.UpdateUserDatum(user);

        var response = new SongPurchaseResponse
        {
            Result = 1,
            TokenCount = tokenCountDict[request.TokenId]
        };

        return Ok(response);
    }
}