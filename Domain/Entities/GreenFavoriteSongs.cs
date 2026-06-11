namespace TaikoLocalServer.Domain.Entities;

public class GreenFavoriteSongs : IAc15FavoriteSong
{
    public uint Baid { get; set; }
    public uint SongNo { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
