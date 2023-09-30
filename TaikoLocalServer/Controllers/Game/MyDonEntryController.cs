﻿using GameDatabase.Entities;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r00_cn/chassis/mydonentry.php")]
[ApiController]
public class MyDonEntryController : BaseController<MyDonEntryController>
{
	private readonly IUserDatumService userDatumService;

	private readonly ICardService cardService;

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
			AccessCode = request.WechatQrStr,
			Baid = newId,
			Password = "",
			Salt = ""
		});

		var newUser = new UserDatum
		{
			Baid = newId,
			MyDonName = request.MydonName,
			MyDonNameLanguage = 0,
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
			TokenCountDict = "{}",
			UnlockedSongIdList = "[]"
		};

		await userDatumService.InsertUserDatum(newUser);

		var response = new MydonEntryResponse
		{
			Result = 1,
			Baid = newId,
			MydonName = request.MydonName,
			MydonNameLanguage = 0
		};

		return Ok(response);
	}
}