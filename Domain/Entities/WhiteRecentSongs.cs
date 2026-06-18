namespace TaikoLocalServer.Domain.Entities;

public partial class WhiteRecentSongs : IAc15RecentSong
{
    public uint Baid { get; set; }
    public uint SongNo { get; set; }
    public DateTime LastPlayed { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
