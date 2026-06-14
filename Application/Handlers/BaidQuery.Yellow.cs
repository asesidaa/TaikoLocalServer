using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<Ac15BaidResponse> HandleYellow(
        Ac15BaidQuery request,
        CancellationToken cancellationToken)
    {
        var card = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
        if (card is null)
        {
            var nextBaid = await context.Cards.Select(existing => existing.Baid)
                .DefaultIfEmpty()
                .MaxAsync(cancellationToken) + 1;

            return new Ac15BaidResponse
            {
                Result = 1,
                IsNewUser = true,
                Baid = nextBaid
            };
        }

        var saveData = await context.UserSaveDataYellow.FindAsync([card.Baid], cancellationToken);
        if (saveData is null)
        {
            return new Ac15BaidResponse
            {
                Result = 1,
                IsNewUser = true,
                Baid = card.Baid
            };
        }

        var userData = await context.UserData.FindAsync([card.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Yellow card baid {card.Baid}.");

        var yellow = gameDataService.Yellow();
        var snapshot = Ac15CatalogSnapshotFactory.FromYellow(yellow);
        var activeShopSeason = snapshot.ItemShopCatalog.ActiveSeason;
        var shopSeasonState = await context.GetOrCreateActiveYellowShopSeasonStateAsync(
            saveData,
            yellow.ItemShopCatalog,
            cancellationToken);
        if (shopSeasonState is not null)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        var limits = Ac15EraProfiles.Yellow.Limits;
        var unlockedShopItems = activeShopSeason is null
            ? new HashSet<(uint ItemType, uint ItemId)>()
            : await context.GetUnlockedYellowShopItemsAsync(card.Baid, activeShopSeason.SeasonId, cancellationToken);
        var costumeFlags = Ac15CustomizationMutation.ApplyActiveShopCostumeLocks(
            saveData,
            activeShopSeason,
            unlockedShopItems,
            Ac15EraProfiles.Yellow.Limits);

        var mydonProfile = new Ac15BaidProfile
        {
            Title = saveData.Title,
            TitlePlateId = ResolveYellowTitlePlateId(saveData),
            ColorFace = saveData.ColorFace,
            ColorBody = saveData.ColorBody,
            ColorLimb = saveData.ColorLimb,
            SelectedCostume = new Ac15CostumeFacts(
                saveData.Costume1,
                saveData.Costume2,
                saveData.Costume3,
                saveData.Costume4,
                saveData.Costume5),
            IsAutoCostumeOn = saveData.IsAutoCostumeOn,
            DefaultToneSetting = saveData.DefaultToneSetting,
            LastPlayDatetime = saveData.LastPlayDatetime == DateTime.UnixEpoch
                ? DateTime.Now.ToString(Constants.DateTimeFormat)
                : saveData.LastPlayDatetime.ToString(Constants.DateTimeFormat)
        };
        var customizationInventory = new Ac15BaidCostumeFlags(
            costumeFlags.CostumeFlg1,
            costumeFlags.CostumeFlg2,
            costumeFlags.CostumeFlg3,
            costumeFlags.CostumeFlg4,
            costumeFlags.CostumeFlg5);
        var shopMedalBalance = new Ac15BaidShopMedals(
            shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal,
            shopSeasonState?.TotalUseDonmedal ?? saveData.TotalUseDonmedal,
            saveData.TotalGetKatsumedal,
            saveData.TotalUseKatsumedal,
            saveData.ItemshopTutorialFlg);
        var danStatus = new Ac15BaidDan(
            saveData.DispDanType == 0 ? 0u : 1u,
            Math.Min(saveData.GotDanMax, limits.MaxNormalDanId),
            Ac15ProtocolBytes.FixedOrZero(saveData.GotDanFlg, limits.DanFlagBytes),
            Ac15ProtocolBytes.FixedOrZero(saveData.GotDanExtraFlg, limits.DanExtraFlagBytes));
        var compatibilityProfile = new Ac15BaidCompatibility(null, saveData.WaiwaiTutorialFlg);

        return new Ac15BaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = card.Baid,
            Identity = new Ac15BaidIdentity(userData.MyDonName, userData.MyDonNameLanguage),
            MydonProfile = mydonProfile,
            CustomizationInventory = customizationInventory,
            ShopMedalBalance = shopMedalBalance,
            DanStatus = danStatus,
            CompatibilityProfile = compatibilityProfile
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
