namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<SongBestDatumGreen> SongBestDataGreen { get; }
    DbSet<SongPlayDatumGreen> SongPlayDataGreen { get; }
    DbSet<UserSaveDataGreen> UserSaveDataGreen { get; }
    DbSet<GhostStageSectionDatumGreen> GhostStageSectionDataGreen { get; }
    DbSet<GreenGhostWinnings> GreenGhostWinnings { get; }
    DbSet<GreenGhostTokens> GreenGhostTokens { get; }
    DbSet<GreenFriends> GreenFriends { get; }
    DbSet<GreenFavoriteSongs> GreenFavoriteSongs { get; }
    DbSet<GreenRecentSongs> GreenRecentSongs { get; }
}
