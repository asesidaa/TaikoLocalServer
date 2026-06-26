using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<Ac15BaidResponse> HandleMomoiro(
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

        var saveData = await context.UserSaveDataMomoiro.FindAsync([card.Baid], cancellationToken);
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
            ?? throw new InvalidOperationException($"User not found for Momoiro card baid {card.Baid}.");

        var limits = Ac15EraProfiles.Momoiro.Limits;
        var mydonProfile = new Ac15BaidProfile
        {
            Title = saveData.Title,
            TitlePlateId = saveData.TitleplateId,
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
            RewardProgress = rewardProgress
        };
    }
}
