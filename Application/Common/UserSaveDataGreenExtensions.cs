namespace TaikoLocalServer.Application.Common;

public static class UserSaveDataGreenExtensions
{
    public static async ValueTask<UserSaveDataGreen> GetOrCreateGreenSaveDataAsync(
        this ITaikoDbContext context,
        uint baid,
        CancellationToken cancellationToken = default)
    {
        var saveData = await context.UserSaveDataGreen.FindAsync([baid], cancellationToken);
        if (saveData is not null)
        {
            return saveData;
        }

        saveData = CreateDefaultGreenSaveData(baid);
        context.UserSaveDataGreen.Add(saveData);
        return saveData;
    }

    public static UserSaveDataGreen CreateDefaultGreenSaveData(uint baid) => new()
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
        CostumeFlg1 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        CostumeFlg2 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        CostumeFlg3 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        CostumeFlg4 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        CostumeFlg5 = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.CostumeFlagBytes),
        ToneFlg = GreenProtocolBytes.CreateFixedBitset([0], GreenProtocolBytes.ToneFlagBytes),
        TitleFlg = new byte[GreenProtocolBytes.TitleFlagBytes],
        OptionFlg = [],
        DefaultOptionSetting = new byte[2],
        DefaultShinSetting = false,
        DefaultToneSetting = 0,
        DispDanType = 1,
        GotDanMax = 0,
        GotDanFlg = new byte[GreenProtocolBytes.DanFlagBytes],
        GotDanExtraFlg = new byte[GreenProtocolBytes.DanExtraFlagBytes],
        DispTaikojukuDan = 0,
        LastPlayDatetime = DateTime.UnixEpoch,
        GhostReleaseInfoFlag = new byte[GreenProtocolBytes.GhostReleaseInfoBytes],
        GhostPlayedSongFlag = new byte[GreenProtocolBytes.GhostPlayedSongBytes],
        IsAutoCostumeOn = false,
        IsDevil = false,
        IsExplain = false,
        IsChallengeCompe = false,
        IsTojiru = true
    };
}
