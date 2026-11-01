
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
}