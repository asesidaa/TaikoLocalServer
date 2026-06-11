using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<CommonBaidResponse> HandleBlue(
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

        var saveData = await context.UserSaveDataBlue.FindAsync([card.Baid], cancellationToken);
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
            ?? throw new InvalidOperationException($"User not found for Blue card baid {card.Baid}.");

        var blue = gameDataService.Blue();
        var snapshot = Ac15CatalogSnapshotFactory.FromBlue(blue);
        var activeShopSeason = blue.ItemShopCatalog.IsEnabled
            ? snapshot.ItemShopCatalog.ActiveSeason
            : null;
        var shopSeasonState = await context.GetOrCreateActiveBlueShopSeasonStateAsync(
            saveData,
            blue.ItemShopCatalog,
            cancellationToken);
        if (shopSeasonState is not null)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        var unlockedShopItems = activeShopSeason is null
            ? new HashSet<(uint ItemType, uint ItemId)>()
            : await context.GetUnlockedBlueShopItemsAsync(card.Baid, activeShopSeason.SeasonId, cancellationToken);
        var costumeFlags = Ac15CustomizationMutation.ApplyActiveShopCostumeLocks(
            saveData,
            activeShopSeason,
            unlockedShopItems,
            Ac15EraProfiles.Blue.Limits);

        return new CommonBaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = card.Baid,
            MyDonName = userData.MyDonName,
            MyDonNameLanguage = userData.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = ResolveBlueTitlePlateId(saveData),
            ColorFace = saveData.ColorFace,
            ColorBody = saveData.ColorBody,
            ColorLimb = saveData.ColorLimb,
            CostumeData = [saveData.Costume1, saveData.Costume2, saveData.Costume3, saveData.Costume4, saveData.Costume5],
            CostumeFlg1 = costumeFlags.CostumeFlg1,
            CostumeFlg2 = costumeFlags.CostumeFlg2,
            CostumeFlg3 = costumeFlags.CostumeFlg3,
            CostumeFlg4 = costumeFlags.CostumeFlg4,
            CostumeFlg5 = costumeFlags.CostumeFlg5,
            TotalGetDonmedal = shopSeasonState?.TotalGetDonmedal ?? 0,
            TotalUseDonmedal = shopSeasonState?.TotalUseDonmedal ?? 0,
            TotalGetKatsumedal = saveData.TotalGetKatsumedal,
            TotalUseKatsumedal = saveData.TotalUseKatsumedal,
            ItemshopTutorialFlg = saveData.ItemshopTutorialFlg,
            IsAutoCostumeOn = saveData.IsAutoCostumeOn,
            DispDanType = saveData.DispDanType == 0 ? 0u : 1u,
            GotDanFlg = BlueProtocolBytes.FixedOrZero(saveData.GotDanFlg, BlueProtocolBytes.DanFlagBytes),
            GotDanMax = Math.Min(saveData.GotDanMax, Ac15EraProfiles.Blue.Limits.MaxNormalDanId),
            GotDanExtraFlg = BlueProtocolBytes.FixedOrZero(saveData.GotDanExtraFlg, BlueProtocolBytes.DanExtraFlagBytes),
            DefaultToneSetting = saveData.DefaultToneSetting,
            WaiwaiTutorialFlg = saveData.WaiwaiTutorialFlg,
            LastPlayDatetime = saveData.LastPlayDatetime == DateTime.UnixEpoch
                ? DateTime.Now.ToString(Constants.DateTimeFormat)
                : saveData.LastPlayDatetime.ToString(Constants.DateTimeFormat)
        };
    }

    private uint ResolveBlueTitlePlateId(UserSaveDataBlue saveData)
    {
        return gameDataService.Blue().GetTitleDictionary().TryGetValue(saveData.TitleplateId, out var title)
               && BlueTitleTextMatches(title, saveData.Title)
            ? title.TitleRarity
            : saveData.TitleplateId;
    }

    private static bool BlueTitleTextMatches(Title title, string selectedTitle)
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
