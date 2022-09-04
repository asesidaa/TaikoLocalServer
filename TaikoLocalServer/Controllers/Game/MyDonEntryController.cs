namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/mydonentry.php")]
[ApiController]
public class MyDonEntryController : BaseController<MyDonEntryController>
{
    private readonly TaikoDbContext context;
    
    public MyDonEntryController(TaikoDbContext context)
    {
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetMyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("MyDonEntry request : {Request}", request.Stringify());

        var newId = context.Cards.Any() ? context.Cards.Max(card => card.Baid) + 1 : 1;
        context.Cards.Add(new Card
        {
            AccessCode = request.AccessCode,
            Baid = newId
        });
        
        context.UserData.Add(new UserDatum
        {
            Baid = newId,
            MyDonName = request.MydonName,
            DisplayDan = true,
            DisplayAchievement = true,
            AchievementDisplayDifficulty = Difficulty.None,
            ColorFace = 0,
            ColorBody = 1,
            ColorLimb = 3,
            FavoriteSongsArray = "{}"
        });
        context.SaveChanges();
        
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