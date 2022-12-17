using GameDatabase.Entities;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/mydonentry.php")]
[ApiController]
public class MyDonEntryController : BaseController<MyDonEntryController>
{
    private readonly ICardService cardService;
    private readonly IUserDatumService userDatumService;

    public MyDonEntryController(IUserDatumService userDatumService, ICardService cardService)
    {
        this.userDatumService = userDatumService;
        this.cardService = cardService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetMyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("MyDonEntry request : {Request}", request.Stringify());

        var newId = cardService.GetNextBaid();
        await cardService.AddCard(new Card
        {
            AccessCode = request.AccessCode,
            Baid = newId,
            Password = ""
        });

        var newUser = new UserDatum
        {
            Baid = newId,
            MyDonName = request.MydonName,
            DisplayDan = true,
            DisplayAchievement = true,
            AchievementDisplayDifficulty = Difficulty.None,
            ColorFace = 0,
            ColorBody = 1,
            ColorLimb = 3,
            FavoriteSongsArray = "[]",
            ToneFlgArray = "[]",
            TitleFlgArray = "[]",
            CostumeFlgArray = "[[],[],[],[],[]]",
            GenericInfoFlgArray = "[]",
            TotalGameCount = 0
        };

        await userDatumService.InsertUserDatum(newUser);

        var response = new MydonEntryResponse
        {
            Result = 1,
            ComSvrResult = 1,
            AccessCode = request.AccessCode,
            Accesstoken = "12345",
            Baid = newId,
            MbId = 1,
            MydonName = request.MydonName,
            CardOwnNum = 1,
            IsPublish = true,
            RegCountryId = request.CountryId,
            PurposeId = 1,
            RegionId = 1
        };

        return Ok(response);
    }
}