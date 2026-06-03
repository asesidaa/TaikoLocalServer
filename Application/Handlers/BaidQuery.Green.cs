using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Domain.Enums;

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
        var dispDanType = saveData.DispDanType == 0 ? 0u : 1u;
        var activeShopSeason = gameDataService.Green().ItemShopCatalog.ActiveSeason;
        var shopSeasonState = activeShopSeason is null
            ? null
            : await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeShopSeason.SeasonId, cancellationToken);
        if (activeShopSeason is not null)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        var unlockedShopItems = activeShopSeason is null
            ? new HashSet<(uint ItemType, uint ItemId)>()
            : await context.GreenShopItemStates
                .Where(row => row.Baid == card.Baid
                    && row.SeasonId == activeShopSeason.SeasonId
                    && row.Status == GreenShopItemStatus.Unlocked)
                .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
                .ToHashSetAsync(cancellationToken);

        IEnumerable<uint> LockedIds(Ac15ShopItemType itemType) => activeShopSeason?.Items
            .Where(item => item.ItemType == itemType && !unlockedShopItems.Contains((item.ItemType.ToProtocolValue(), item.ItemId)))
            .Select(item => item.ItemId) ?? [];

        return new CommonBaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = card.Baid,
            MyDonName = userData.MyDonName,
            MyDonNameLanguage = userData.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = ResolveGreenTitlePlateId(saveData),
            ColorFace = saveData.ColorFace,
            ColorBody = saveData.ColorBody,
            ColorLimb = saveData.ColorLimb,
            CostumeData = [saveData.Costume1, saveData.Costume2, saveData.Costume3, saveData.Costume4, saveData.Costume5],
            CostumeFlg1 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg1, LockedIds(Ac15ShopItemType.Kigurumi), GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg2 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg2, LockedIds(Ac15ShopItemType.Head), GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg3 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg3, LockedIds(Ac15ShopItemType.Body), GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg4 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg4, LockedIds(Ac15ShopItemType.Face), GreenProtocolBytes.CostumeFlagBytes),
            CostumeFlg5 = GreenShopUnlocks.ClearBits(saveData.CostumeFlg5, LockedIds(Ac15ShopItemType.Puchi), GreenProtocolBytes.CostumeFlagBytes),
            TotalGetDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal,
            TotalUseDonmedal = shopSeasonState?.TotalUseDonmedal ?? saveData.TotalUseDonmedal,
            TotalGetKatsumedal = saveData.TotalGetKatsumedal,
            TotalUseKatsumedal = saveData.TotalUseKatsumedal,
            ItemshopTutorialFlg = saveData.ItemshopTutorialFlg,
            IsAutoCostumeOn = saveData.IsAutoCostumeOn,
            DispDanType = dispDanType,
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

    private uint ResolveGreenTitlePlateId(UserSaveDataGreen saveData)
    {
        return gameDataService.Green().GetTitleDictionary().TryGetValue(saveData.TitleplateId, out var title)
               && GreenTitleTextMatches(title, saveData.Title)
            ? title.TitleRarity
            : saveData.TitleplateId;
    }

    private static bool GreenTitleTextMatches(Title title, string selectedTitle)
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
