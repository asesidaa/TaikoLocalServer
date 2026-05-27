namespace TaikoLocalServer.Domain.Entities;

public partial class BlueFavoriteSongs
{
    public uint Baid { get; set; }
    public uint SongNo { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
