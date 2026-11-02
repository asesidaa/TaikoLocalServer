namespace TaikoLocalServer.Domain.Entities;

public interface IAc15RecentSong
{
    uint Baid { get; set; }

    uint SongNo { get; set; }

    DateTime LastPlayed { get; set; }
}
