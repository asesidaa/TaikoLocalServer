namespace SharedProject.Models.Requests;

public class SetFavoriteRequest
{
    public ulong Baid { get; set; }
    public uint SongId { get; set; }
    public bool IsFavorite { get; set; }
}