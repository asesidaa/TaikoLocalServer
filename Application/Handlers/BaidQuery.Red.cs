using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<Ac15BaidResponse> HandleRed(
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

        var saveData = await context.UserSaveDataRed.FindAsync([card.Baid], cancellationToken);
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
            ?? throw new InvalidOperationException($"User not found for Red card baid {card.Baid}.");

        var limits = Ac15EraProfiles.Red.Limits;
        var mydonProfile = new Ac15BaidProfile
        {
            Title = saveData.Title,
            TitlePlateId = ResolveRedTitlePlateId(saveData),
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
            Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg1, limits.CostumeFlagBytes),
            Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg2, limits.CostumeFlagBytes),
            Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg3, limits.CostumeFlagBytes),
            Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg4, limits.CostumeFlagBytes),
            Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg5, limits.CostumeFlagBytes));
        var danStatus = new Ac15BaidDan(
            saveData.DispDanType == 0 ? 0u : 1u,
            Math.Min(saveData.GotDanMax, limits.MaxNormalDanId),
            Ac15ProtocolBytes.FixedOrZero(saveData.GotDanFlg, limits.DanFlagBytes),
            Ac15ProtocolBytes.FixedOrZero(saveData.GotDanExtraFlg, limits.DanExtraFlagBytes));
        var compatibilityProfile = new Ac15BaidCompatibility(null, null);
        var rewardProgress = new Ac15BaidReward(saveData.RewardPtn);

        return new Ac15BaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = card.Baid,
            Identity = new Ac15BaidIdentity(userData.MyDonName, userData.MyDonNameLanguage),
            MydonProfile = mydonProfile,
            CustomizationInventory = customizationInventory,
            DanStatus = danStatus,
            CompatibilityProfile = compatibilityProfile,
            RewardProgress = rewardProgress
        };
    }

    private uint ResolveRedTitlePlateId(UserSaveDataRed saveData)
    {
        return gameDataService.Red().GetTitleDictionary().TryGetValue(saveData.TitleplateId, out var title)
               && RedTitleTextMatches(title, saveData.Title)
            ? title.TitleRarity
            : saveData.TitleplateId;
    }

    private static bool RedTitleTextMatches(Title title, string selectedTitle)
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
