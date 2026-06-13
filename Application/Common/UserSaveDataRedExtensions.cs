using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Common;

public static class UserSaveDataRedExtensions
{
    public static async ValueTask<UserSaveDataRed> GetOrCreateRedSaveDataAsync(
        this ITaikoDbContext context,
        uint baid,
        CancellationToken cancellationToken = default)
    {
        var saveData = await context.UserSaveDataRed.FindAsync([baid], cancellationToken);
        if (saveData is not null)
        {
            return saveData;
        }

        saveData = CreateDefaultRedSaveData(baid);
        context.UserSaveDataRed.Add(saveData);
        return saveData;
    }

    public static UserSaveDataRed CreateDefaultRedSaveData(uint baid)
    {
        var limits = Ac15EraProfiles.Red.Limits;
        return new UserSaveDataRed
        {
            Baid = baid,
            Title = string.Empty,
            TitleplateId = 0,
            ColorFace = 0,
            ColorBody = 1,
            ColorLimb = 3,
            Costume1 = 0,
            Costume2 = 0,
            Costume3 = 0,
            Costume4 = 0,
            Costume5 = 0,
            CostumeFlg1 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
            CostumeFlg2 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
            CostumeFlg3 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
            CostumeFlg4 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
            CostumeFlg5 = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
            ToneFlg = Ac15ProtocolBytes.CreateFixedBitset([0], limits.ToneFlagBytes),
            TitleFlg = new byte[limits.TitleFlagBytes],
            ReleaseSongFlg = new byte[limits.SongFlagBytes],
            OptionFlg = [],
            DefaultOptionSetting = new byte[2],
            DefaultShinSetting = false,
            DefaultToneSetting = 0,
            DispDanType = 1,
            GotDanMax = 0,
            GotDanFlg = new byte[limits.DanFlagBytes],
            GotDanExtraFlg = new byte[limits.DanExtraFlagBytes],
            DispTaikojukuDan = 0,
            RewardPtn = 0,
            RewardProgress = 0,
            DifficultyTutorialFlg = 0,
            IsAutoCostumeOn = true,
            IsDevil = false,
            IsExplain = false,
            IsChallengeCompe = false,
            IsTojiru = true,
            LastPlayDatetime = DateTime.UnixEpoch
        };
    }
}
