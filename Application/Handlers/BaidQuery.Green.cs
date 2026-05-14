namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<CommonBaidResponse> HandleGreen(
        BaidQuery request,
        CancellationToken cancellationToken)
    {
        var card = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
        if (card is null)
        {
            var nextBaid = await context.Cards.Select(existing => existing.Baid)
                .DefaultIfEmpty()
                .MaxAsync(cancellationToken) + 1;

            return new CommonBaidResponse
            {
                Result = 1,
                IsNewUser = true,
                Baid = nextBaid
            };
        }

        var saveData = await context.UserSaveDataGreen.FindAsync([card.Baid], cancellationToken);
        if (saveData is null)
        {
            return new CommonBaidResponse
            {
                Result = 1,
                IsNewUser = true,
                Baid = card.Baid
            };
        }

        var userData = await context.UserData.FindAsync([card.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Green card baid {card.Baid}.");

        var gotDanFlg = GreenProtocolBytes.FixedOrZero(saveData.GotDanFlg, GreenProtocolBytes.DanFlagBytes);
        var gotDanExtraFlg = GreenProtocolBytes.FixedOrZero(saveData.GotDanExtraFlg, GreenProtocolBytes.DanExtraFlagBytes);
        var gotDanMax = Math.Min(saveData.GotDanMax, GreenDanHelpers.MaxNormalDanId);

        return new CommonBaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = card.Baid,
            MyDonName = userData.MyDonName,
            MyDonNameLanguage = userData.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = saveData.TitleplateId,
            ColorFace = saveData.ColorFace,
            ColorBody = saveData.ColorBody,
            ColorLimb = saveData.ColorLimb,
            CostumeData = [saveData.Costume1, saveData.Costume2, saveData.Costume3, saveData.Costume4, saveData.Costume5],
            CostumeFlg1 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg2 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg2, GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg3 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg3, GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg4 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg4, GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg5 = GreenProtocolBytes.FixedOrZero(saveData.CostumeFlg5, GreenProtocolBytes.CostumeFlagBytes),
            TotalGetDonmedal = saveData.TotalGetDonmedal,
            TotalUseDonmedal = saveData.TotalUseDonmedal,
            TotalGetKatsumedal = saveData.TotalGetKatsumedal,
            TotalUseKatsumedal = saveData.TotalUseKatsumedal,
            ItemshopTutorialFlg = saveData.ItemshopTutorialFlg,
            IsAutoCostumeOn = saveData.IsAutoCostumeOn,
            DispDanType = saveData.DispDanType,
            GotDanFlg = gotDanFlg,
            GotDanMax = gotDanMax,
            GotDanExtraFlg = gotDanExtraFlg,
            DefaultToneSetting = saveData.DefaultToneSetting,
            WaiwaiTutorialFlg = saveData.WaiwaiTutorialFlg,
            LastPlayDatetime = saveData.LastPlayDatetime == DateTime.UnixEpoch
                ? DateTime.Now.ToString(Constants.DateTimeFormat)
                : saveData.LastPlayDatetime.ToString(Constants.DateTimeFormat)
        };
    }
}
