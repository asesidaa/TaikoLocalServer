namespace TaikoLocalServer.Domain.Entities;

public partial class WhiteFavoriteSongs : IAc15FavoriteSong
{
    public uint Baid { get; set; }
    public uint SongNo { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
