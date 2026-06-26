namespace TaikoLocalServer.Domain.Entities;

public partial class MomoiroFavoriteSongs : IAc15FavoriteSong
{
    public uint Baid { get; set; }
    public uint SongNo { get; set; }
    public int DisplayOrder { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
