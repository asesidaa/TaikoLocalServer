namespace TaikoLocalServer.Application.Common;

public static class UserSaveDataBlueExtensions
{
    public static async ValueTask<UserSaveDataBlue> GetOrCreateBlueSaveDataAsync(
        this ITaikoDbContext context,
        uint baid,
        CancellationToken cancellationToken = default)
    {
        var saveData = await context.UserSaveDataBlue.FindAsync([baid], cancellationToken);
        if (saveData is not null)
        {
            return saveData;
        }

        saveData = CreateDefaultBlueSaveData(baid);
        context.UserSaveDataBlue.Add(saveData);
        return saveData;
    }

    public static UserSaveDataBlue CreateDefaultBlueSaveData(uint baid) => new()
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
        CostumeFlg1 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        CostumeFlg2 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        CostumeFlg3 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        CostumeFlg4 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        CostumeFlg5 = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.CostumeFlagBytes),
        ToneFlg = BlueProtocolBytes.CreateFixedBitset([0], BlueProtocolBytes.ToneFlagBytes),
        TitleFlg = new byte[BlueProtocolBytes.TitleFlagBytes],
        OptionFlg = [],
        DefaultOptionSetting = new byte[2],
        DefaultShinSetting = false,
        DefaultToneSetting = 0,
        DispDanType = 1,
        GotDanMax = 0,
        GotDanFlg = new byte[BlueProtocolBytes.DanFlagBytes],
        GotDanExtraFlg = new byte[BlueProtocolBytes.DanExtraFlagBytes],
        DispTaikojukuDan = 0,
        IsAutoCostumeOn = true,
        IsDevil = false,
        IsExplain = false,
        IsChallengeCompe = false,
        IsTojiru = true,
        LastPlayDatetime = DateTime.UnixEpoch
    };
}
