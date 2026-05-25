using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<SongBestDatumGreen> SongBestDataGreen { get; set; } = null!;
    public virtual DbSet<SongPlayDatumGreen> SongPlayDataGreen { get; set; } = null!;
    public virtual DbSet<DanScoreDatumGreen> DanScoreDataGreen { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatumGreen> DanStageScoreDataGreen { get; set; } = null!;
    public virtual DbSet<UserSaveDataGreen> UserSaveDataGreen { get; set; } = null!;
    public virtual DbSet<GhostStageSectionDatumGreen> GhostStageSectionDataGreen { get; set; } = null!;
    public virtual DbSet<GreenGhostWinnings> GreenGhostWinnings { get; set; } = null!;
    public virtual DbSet<GreenGhostTokens> GreenGhostTokens { get; set; } = null!;
    public virtual DbSet<GreenFriends> GreenFriends { get; set; } = null!;
    public virtual DbSet<GreenFavoriteSongs> GreenFavoriteSongs { get; set; } = null!;
    public virtual DbSet<GreenRecentSongs> GreenRecentSongs { get; set; } = null!;
    public virtual DbSet<GreenShopSeasonState> GreenShopSeasonStates { get; set; } = null!;
    public virtual DbSet<GreenShopItemState> GreenShopItemStates { get; set; } = null!;

    partial void OnModelCreatingGreen(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SongBestDatumGreen>(entity =>
        {
            entity.ToTable("SongBestDatum_Green");
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty, e.IsShin });
            entity.HasIndex(e => new { e.SongId, e.Difficulty, e.BestScore });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.BestCrown).HasConversion<uint>();
        });

        modelBuilder.Entity<SongPlayDatumGreen>(entity =>
        {
            entity.ToTable("SongPlayDatum_Green");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Baid, e.SongId, e.Difficulty, e.PlayTime });
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlayTime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.Crown).HasConversion<uint>();
        });

        modelBuilder.Entity<UserSaveDataGreen>(entity =>
        {
            entity.ToTable("UserSaveData_Green");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanScoreDatumGreen>(entity =>
        {
            entity.ToTable("DanScoreDatum_Green");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra });

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.ClearGrade)
                .HasConversion<uint>()
                .HasDefaultValue(GreenDanClearGrade.NotClear);
        });

        modelBuilder.Entity<DanStageScoreDatumGreen>(entity =>
        {
            entity.ToTable("DanStageScoreDatum_Green");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra, e.StageIndex });

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasForeignKey(d => new { d.Baid, d.DanId, d.IsExtra })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GhostStageSectionDatumGreen>(entity =>
        {
            entity.ToTable("GhostStageSectionDatum_Green");
            entity.HasKey(e => new { e.PlayId, e.SectionNo });
            entity.HasOne(d => d.Parent)
                .WithMany()
                .HasForeignKey(d => d.PlayId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenGhostWinnings>(entity =>
        {
            entity.ToTable("GreenGhostWinnings");
            entity.HasKey(e => new { e.Baid, e.LevelId });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenGhostTokens>(entity =>
        {
            entity.ToTable("GreenGhostTokens");
            entity.HasKey(e => new { e.Baid, e.TokenId });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenFriends>(entity =>
        {
            entity.ToTable("GreenFriends");
            entity.HasKey(e => new { e.Baid, e.FriendBaid });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenFavoriteSongs>(entity =>
        {
            entity.ToTable("GreenFavoriteSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenRecentSongs>(entity =>
        {
            entity.ToTable("GreenRecentSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.Property(e => e.LastPlayed).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenShopSeasonState>(entity =>
        {
            entity.ToTable("GreenShopSeasonStates");
            entity.HasKey(e => new { e.Baid, e.SeasonId });
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenShopItemState>(entity =>
        {
            entity.ToTable("GreenShopItemStates");
            entity.HasKey(e => new { e.Baid, e.SeasonId, e.ItemType, e.ItemId });
            entity.Property(e => e.Status).HasConversion<uint>();
            entity.Property(e => e.PurchasedAt).HasColumnType("datetime");
            entity.Property(e => e.UnlockedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
