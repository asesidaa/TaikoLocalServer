using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Application.Common;

public static class GreenSeedDataService
{
    public static IReadOnlyList<SongBestDatumGreen> CreateFakeBestSeeds(
        uint baid,
        IReadOnlyList<GreenMusicInfoEntry> musicInfoFileOrder)
    {
        var seeds = new List<SongBestDatumGreen>();

        AddSeed(seeds, baid, musicInfoFileOrder.ElementAtOrDefault(0), Difficulty.Easy, 123450, CrownType.Clear);
        AddSeed(seeds, baid, musicInfoFileOrder.ElementAtOrDefault(1), Difficulty.Normal, 234560, CrownType.Gold);
        AddSeed(seeds, baid, musicInfoFileOrder.ElementAtOrDefault(2), Difficulty.Hard, 345670, CrownType.Dondaful);
        AddSeed(seeds, baid, musicInfoFileOrder.ElementAtOrDefault(3), Difficulty.Oni, 456780, CrownType.Clear);

        var uraSong = musicInfoFileOrder.FirstOrDefault(song => song.HasExtreme);
        AddSeed(seeds, baid, uraSong, Difficulty.UraOni, 567890, CrownType.Gold);

        return seeds;
    }

    public static bool GrantFirstFakeDanIfNeeded(UserSaveDataGreen saveData)
    {
        saveData.GotDanFlg = GreenProtocolBytes.FixedOrZero(saveData.GotDanFlg, GreenProtocolBytes.DanFlagBytes);

        if ((saveData.GotDanFlg[0] & 0b11) != 0)
        {
            return false;
        }

        GreenProtocolBytes.SetTwoBitValue(saveData.GotDanFlg, 0, 1);
        saveData.GotDanMax = Math.Max(saveData.GotDanMax, 1);
        saveData.DispTaikojukuDan = Math.Max(saveData.DispTaikojukuDan, 1);
        return true;
    }

    private static void AddSeed(
        List<SongBestDatumGreen> seeds,
        uint baid,
        GreenMusicInfoEntry? song,
        Difficulty difficulty,
        uint score,
        CrownType crown)
    {
        if (song is null)
        {
            return;
        }

        seeds.Add(new SongBestDatumGreen
        {
            Baid = baid,
            SongId = song.SongNo,
            Difficulty = difficulty,
            BestScore = score,
            BestRate = 0,
            BestCrown = crown
        });
    }
}
