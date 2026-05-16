namespace TaikoLocalServer.Tests.Green;

public sealed class GreenMusicInfoEntryTests
{
    [Fact]
    public void WithExpression_CanPopulateStarsWithoutMutatingOriginal()
    {
        var entry = new GreenMusicInfoEntry
        {
            MusicId = "tank",
            SongNo = 105
        };
        var set = new GreenStarSet(3, 5, 6, 6, 9);

        var enriched = entry with
        {
            StarEasy = set.Easy,
            StarNormal = set.Normal,
            StarHard = set.Hard,
            StarOni = set.Oni,
            StarUra = set.Ura
        };

        Assert.Equal(0u, entry.StarEasy);
        Assert.Equal(0u, entry.StarNormal);
        Assert.Equal(0u, entry.StarHard);
        Assert.Equal(0u, entry.StarOni);
        Assert.Equal(0u, entry.StarUra);

        Assert.Equal(3u, enriched.StarEasy);
        Assert.Equal(5u, enriched.StarNormal);
        Assert.Equal(6u, enriched.StarHard);
        Assert.Equal(6u, enriched.StarOni);
        Assert.Equal(9u, enriched.StarUra);
    }
}
