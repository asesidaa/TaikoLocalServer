namespace TaikoLocalServer.Tests.Green;

public sealed class Ac15MusicInfoEntryTests
{
    [Fact]
    public void WithExpression_CanPopulateStarsWithoutMutatingOriginal()
    {
        var entry = new Ac15MusicInfoEntry
        {
            MusicId = "tank",
            SongNo = 105
        };
        var set = new Ac15StarSet(3, 5, 6, 6, 9);

        var enriched = entry with
        {
            StarEasy = set.Easy,
            StarNormal = set.Normal,
            StarHard = set.Hard,
            StarOni = set.Oni,
            StarUra = set.Ura
        };

        Assert.Equal((byte)0, entry.StarEasy);
        Assert.Equal((byte)0, entry.StarNormal);
        Assert.Equal((byte)0, entry.StarHard);
        Assert.Equal((byte)0, entry.StarOni);
        Assert.Equal((byte)0, entry.StarUra);

        Assert.Equal((byte)3, enriched.StarEasy);
        Assert.Equal((byte)5, enriched.StarNormal);
        Assert.Equal((byte)6, enriched.StarHard);
        Assert.Equal((byte)6, enriched.StarOni);
        Assert.Equal((byte)9, enriched.StarUra);
    }
}
