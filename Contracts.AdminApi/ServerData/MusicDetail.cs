
namespace TaikoLocalServer.Contracts.AdminApi.ServerData;

public class MusicDetail
{
    public uint SongId { get; set; }
    public int Index { get; set; }

    public string SongName { get; set; } = string.Empty;
    public string SongNameEN { get; set; } = string.Empty;
    public string SongNameCN { get; set; } = string.Empty;
    public string SongNameKO { get; set; } = string.Empty;

    public string ArtistName { get; set; } = string.Empty;
    public string ArtistNameEN { get; set; } = string.Empty;
    public string ArtistNameCN { get; set; } = string.Empty;
    public string ArtistNameKO { get; set; } = string.Empty;

    public SongGenre Genre { get; set; }
    public int StarEasy { get; set; }
    public int StarNormal { get; set; }
    public int StarHard { get; set; }
    public int StarOni { get; set; }
    public int StarUra { get; set; }

    public bool IsFavorite { get; set; } = false;
    /// <summary>The game marks it new (music_attribute); false when that table isn't there.</summary>
    public bool IsNew { get; set; }
    /// <summary>A Sou-uchi chart (music_attribute): listed only for players with "Show Sou-uchi songs" on, as in the game.</summary>
    public bool IsSouUchi { get; set; }

    /// <summary>
    /// The title in a player's language: their profile's Language (MyDonNameLanguage: 0 Japanese, 1 English, 2 Chinese
    /// Traditional, 3 Korean, 4 Chinese Simplified, which the word list has no column for, so Traditional). Japanese
    /// when the song has no name in that language.
    /// </summary>
    public string Title(uint language) =>
        (language switch { 1 => SongNameEN, 2 or 4 => SongNameCN, 3 => SongNameKO, _ => "" }) is { Length: > 0 } t ? t : SongName;

    /// <summary>The line under the title: the word list's subtitle (song_sub, loaded as ArtistName) in the same language,
    /// else the Japanese one; empty when the song has none.</summary>
    public string Subtitle(uint language) =>
        (language switch { 1 => ArtistNameEN, 2 or 4 => ArtistNameCN, 3 => ArtistNameKO, _ => "" }) is { Length: > 0 } s ? s : ArtistName;
}