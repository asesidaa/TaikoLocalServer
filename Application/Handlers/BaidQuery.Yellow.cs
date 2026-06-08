using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<CommonBaidResponse> HandleYellow(
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

        var saveData = await context.UserSaveDataYellow.FindAsync([card.Baid], cancellationToken);
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
            ?? throw new InvalidOperationException($"User not found for Yellow card baid {card.Baid}.");

        var yellow = gameDataService.Yellow();
        var shopSeasonState = await context.GetOrCreateActiveYellowShopSeasonStateAsync(
            saveData,
            yellow.ItemShopCatalog,
            cancellationToken);
        if (shopSeasonState is not null)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        var limits = Ac15EraProfiles.Yellow.Limits;
        return new CommonBaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = card.Baid,
            MyDonName = userData.MyDonName,
            MyDonNameLanguage = userData.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = ResolveYellowTitlePlateId(saveData),
            ColorFace = saveData.ColorFace,
            ColorBody = saveData.ColorBody,
            ColorLimb = saveData.ColorLimb,
            CostumeData = [saveData.Costume1, saveData.Costume2, saveData.Costume3, saveData.Costume4, saveData.Costume5],
            CostumeFlg1 = Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg1, limits.CostumeFlagBytes),
            CostumeFlg2 = Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg2, limits.CostumeFlagBytes),
            CostumeFlg3 = Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg3, limits.CostumeFlagBytes),
            CostumeFlg4 = Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg4, limits.CostumeFlagBytes),
            CostumeFlg5 = Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg5, limits.CostumeFlagBytes),
            TotalGetDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal,
            TotalUseDonmedal = shopSeasonState?.TotalUseDonmedal ?? saveData.TotalUseDonmedal,
            TotalGetKatsumedal = saveData.TotalGetKatsumedal,
            TotalUseKatsumedal = saveData.TotalUseKatsumedal,
            ItemshopTutorialFlg = saveData.ItemshopTutorialFlg,
            IsAutoCostumeOn = saveData.IsAutoCostumeOn,
            DispDanType = saveData.DispDanType == 0 ? 0u : 1u,
            GotDanFlg = Ac15ProtocolBytes.FixedOrZero(saveData.GotDanFlg, limits.DanFlagBytes),
            GotDanMax = Math.Min(saveData.GotDanMax, limits.MaxNormalDanId),
            GotDanExtraFlg = Ac15ProtocolBytes.FixedOrZero(saveData.GotDanExtraFlg, limits.DanExtraFlagBytes),
            DefaultToneSetting = saveData.DefaultToneSetting,
            WaiwaiTutorialFlg = saveData.WaiwaiTutorialFlg,
            LastPlayDatetime = saveData.LastPlayDatetime == DateTime.UnixEpoch
                ? DateTime.Now.ToString(Constants.DateTimeFormat)
                : saveData.LastPlayDatetime.ToString(Constants.DateTimeFormat)
        };
    }

    private uint ResolveYellowTitlePlateId(UserSaveDataYellow saveData)
    {
        return gameDataService.Yellow().GetTitleDictionary().TryGetValue(saveData.TitleplateId, out var title)
               && YellowTitleTextMatches(title, saveData.Title)
            ? title.TitleRarity
            : saveData.TitleplateId;
    }

    private static bool YellowTitleTextMatches(Title title, string selectedTitle)
    {
        if (string.IsNullOrWhiteSpace(selectedTitle))
        {
            return false;
        }

        return string.Equals(title.TitleName, selectedTitle, StringComparison.Ordinal)
               || string.Equals(title.TitleNameEN, selectedTitle, StringComparison.Ordinal)
               || string.Equals(title.TitleNameCN, selectedTitle, StringComparison.Ordinal)
               || string.Equals(title.TitleNameKO, selectedTitle, StringComparison.Ordinal);
    }
}
