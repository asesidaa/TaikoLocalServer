using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataYellow> UserSaveDataYellow { get; set; } = null!;
    public virtual DbSet<SongBestDatumYellow> SongBestDataYellow { get; set; } = null!;
    public virtual DbSet<SongPlayDatumYellow> SongPlayDataYellow { get; set; } = null!;
    public virtual DbSet<YellowFavoriteSongs> YellowFavoriteSongs { get; set; } = null!;
    public virtual DbSet<YellowRecentSongs> YellowRecentSongs { get; set; } = null!;
    public virtual DbSet<DanScoreDatumYellow> DanScoreDataYellow { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatumYellow> DanStageScoreDataYellow { get; set; } = null!;
    public virtual DbSet<YellowShopSeasonState> YellowShopSeasonStates { get; set; } = null!;
    public virtual DbSet<YellowShopItemState> YellowShopItemStates { get; set; } = null!;
    public virtual DbSet<YellowTokkunStageResult> YellowTokkunStageResults { get; set; } = null!;

    partial void OnModelCreatingYellow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSaveDataYellow>(entity =>
        {
            entity.ToTable("UserSaveData_Yellow");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SongBestDatumYellow>(entity =>
        {
            entity.ToTable("SongBestDatum_Yellow");
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

        modelBuilder.Entity<SongPlayDatumYellow>(entity =>
        {
            entity.ToTable("SongPlayDatum_Yellow");
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

        modelBuilder.Entity<YellowFavoriteSongs>(entity =>
        {
            entity.ToTable("YellowFavoriteSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<YellowRecentSongs>(entity =>
        {
            entity.ToTable("YellowRecentSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.Property(e => e.LastPlayed).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanScoreDatumYellow>(entity =>
        {
            entity.ToTable("DanScoreDatum_Yellow");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra });
            entity.HasIndex(e => e.MedleyUniqueId);
            entity.Property(e => e.ClearGrade)
                .HasConversion<uint>()
                .HasDefaultValue(Ac15DanClearGrade.NotClear);
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanStageScoreDatumYellow>(entity =>
        {
            entity.ToTable("DanStageScoreDatum_Yellow");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra, e.StageIndex });
            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasForeignKey(d => new { d.Baid, d.DanId, d.IsExtra })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<YellowShopSeasonState>(entity =>
        {
            entity.ToTable("YellowShopSeasonStates");
            entity.HasKey(e => new { e.Baid, e.SeasonId });
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<YellowShopItemState>(entity =>
        {
            entity.ToTable("YellowShopItemStates");
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

        modelBuilder.Entity<YellowTokkunStageResult>(entity =>
        {
            entity.ToTable("YellowTokkunStageResults");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.Baid, e.PlayDatetime });
            entity.Property(e => e.PlayDatetime).IsRequired();
            entity.Property(e => e.BanacoinDatetime).IsRequired();
            entity.Property(e => e.TookunSongnoesJson).IsRequired();
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
