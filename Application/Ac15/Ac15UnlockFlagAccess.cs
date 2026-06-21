namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15UnlockFlagAccess<TSave>(
    Action<TSave, IEnumerable<uint>>? ReleaseSongs,
    Action<TSave, IEnumerable<uint>> Tones,
    Action<TSave, IEnumerable<uint>> Titles,
    Action<TSave, IEnumerable<uint>> Costume1,
    Action<TSave, IEnumerable<uint>> Costume2,
    Action<TSave, IEnumerable<uint>> Costume3,
    Action<TSave, IEnumerable<uint>> Costume4,
    Action<TSave, IEnumerable<uint>> Costume5);

public static class Ac15UnlockFlagAccess
{
    public static Ac15UnlockFlagAccess<UserSaveDataBlue> Blue { get; } = new(
        (save, ids) => save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, ids, BlueProtocolBytes.SongFlagBytes),
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, BlueProtocolBytes.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, BlueProtocolBytes.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, BlueProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, BlueProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, BlueProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, BlueProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, BlueProtocolBytes.CostumeFlagBytes));

    public static Ac15UnlockFlagAccess<UserSaveDataGreen> Green { get; } = new(
        ReleaseSongs: null,
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, GreenProtocolBytes.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, GreenProtocolBytes.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, GreenProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, GreenProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, GreenProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, GreenProtocolBytes.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, GreenProtocolBytes.CostumeFlagBytes));

    public static Ac15UnlockFlagAccess<UserSaveDataYellow> Yellow { get; } = new(
        (save, ids) => save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, ids, Ac15EraProfiles.Yellow.Limits.SongFlagBytes),
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, Ac15EraProfiles.Yellow.Limits.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, Ac15EraProfiles.Yellow.Limits.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes));

    public static Ac15UnlockFlagAccess<UserSaveDataRed> Red { get; } = new(
        (save, ids) => save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, ids, Ac15EraProfiles.Red.Limits.SongFlagBytes),
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, Ac15EraProfiles.Red.Limits.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, Ac15EraProfiles.Red.Limits.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, Ac15EraProfiles.Red.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, Ac15EraProfiles.Red.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, Ac15EraProfiles.Red.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, Ac15EraProfiles.Red.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, Ac15EraProfiles.Red.Limits.CostumeFlagBytes));

    public static Ac15UnlockFlagAccess<UserSaveDataWhite> White { get; } = new(
        (save, ids) => save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, ids, Ac15EraProfiles.White.Limits.SongFlagBytes),
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, Ac15EraProfiles.White.Limits.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, Ac15EraProfiles.White.Limits.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, Ac15EraProfiles.White.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, Ac15EraProfiles.White.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, Ac15EraProfiles.White.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, Ac15EraProfiles.White.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, Ac15EraProfiles.White.Limits.CostumeFlagBytes));

    public static Ac15UnlockFlagAccess<UserSaveDataMurasaki> Murasaki { get; } = new(
        (save, ids) => save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, ids, Ac15EraProfiles.Murasaki.Limits.SongFlagBytes),
        (save, ids) => save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, ids, Ac15EraProfiles.Murasaki.Limits.ToneFlagBytes),
        (save, ids) => save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, ids, Ac15EraProfiles.Murasaki.Limits.TitleFlagBytes),
        (save, ids) => save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, ids, Ac15EraProfiles.Murasaki.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg2 = Ac15ProtocolBytes.SetBits(save.CostumeFlg2, ids, Ac15EraProfiles.Murasaki.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg3 = Ac15ProtocolBytes.SetBits(save.CostumeFlg3, ids, Ac15EraProfiles.Murasaki.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg4 = Ac15ProtocolBytes.SetBits(save.CostumeFlg4, ids, Ac15EraProfiles.Murasaki.Limits.CostumeFlagBytes),
        (save, ids) => save.CostumeFlg5 = Ac15ProtocolBytes.SetBits(save.CostumeFlg5, ids, Ac15EraProfiles.Murasaki.Limits.CostumeFlagBytes));
}
