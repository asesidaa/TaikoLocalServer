namespace TaikoWebUI.Shared.Models;

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
}