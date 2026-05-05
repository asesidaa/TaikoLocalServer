namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class SetFavoriteRequest
{
    public uint Baid { get; set; }
    public uint SongId { get; set; }
    public bool IsFavorite { get; set; }
}