using TaikoLocalServer.Contracts.AdminApi.ServerData;

namespace TaikoLocalServer.Tests.Nijiiro;

// Song titles in a player's language (their profile Language, or Settings › Display's choice): the word list has
// Japanese, English, Chinese (Traditional) and Korean, and the line under the title is the song's own subtitle
// (song_sub) in that language. A missing name or subtitle falls back to the Japanese one.
public sealed class SongTitleTests
{
    private static readonly MusicDetail Song = new()
    {
        SongName = "千本桜", SongNameEN = "Senbonzakura", SongNameCN = "千本櫻", SongNameKO = "",
        ArtistName = "黒うさP feat.初音ミク", ArtistNameEN = "Kurousa-P feat. Hatsune Miku", ArtistNameCN = "", ArtistNameKO = "",
    };

    [Fact]
    public void TheTitle_FollowsTheLanguage_AndFallsBackToJapanese()
    {
        Assert.Equal("千本桜", Song.Title(0));
        Assert.Equal("Senbonzakura", Song.Title(1));
        Assert.Equal("千本櫻", Song.Title(2));
        Assert.Equal("千本桜", Song.Title(3)); // no Korean name
        Assert.Equal("千本櫻", Song.Title(4)); // Simplified: the word list's Chinese column
    }

    [Fact]
    public void TheLineUnder_IsTheSubtitleInTheSameLanguage()
    {
        Assert.Equal("黒うさP feat.初音ミク", Song.Subtitle(0));
        Assert.Equal("Kurousa-P feat. Hatsune Miku", Song.Subtitle(1));
        Assert.Equal("黒うさP feat.初音ミク", Song.Subtitle(2)); // no Chinese subtitle: the Japanese one
        Assert.Equal("", new MusicDetail { SongName = "No subtitle" }.Subtitle(1));
    }
}
