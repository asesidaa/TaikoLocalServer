namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataYellow> UserSaveDataYellow { get; }
    DbSet<SongBestDatumYellow> SongBestDataYellow { get; }
    DbSet<SongPlayDatumYellow> SongPlayDataYellow { get; }
    DbSet<YellowFavoriteSongs> YellowFavoriteSongs { get; }
    DbSet<YellowRecentSongs> YellowRecentSongs { get; }
}
