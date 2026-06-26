namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataMomoiro> UserSaveDataMomoiro { get; }
    DbSet<SongBestDatumMomoiro> SongBestDataMomoiro { get; }
    DbSet<MomoiroFavoriteSongs> MomoiroFavoriteSongs { get; }
    DbSet<MomoiroRecentSongs> MomoiroRecentSongs { get; }
}
