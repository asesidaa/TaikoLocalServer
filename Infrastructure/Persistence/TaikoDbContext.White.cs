using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataWhite> UserSaveDataWhite { get; set; } = null!;
    public virtual DbSet<SongBestDatumWhite> SongBestDataWhite { get; set; } = null!;
    public virtual DbSet<SongPlayDatumWhite> SongPlayDataWhite { get; set; } = null!;
    public virtual DbSet<WhiteFavoriteSongs> WhiteFavoriteSongs { get; set; } = null!;
    public virtual DbSet<WhiteRecentSongs> WhiteRecentSongs { get; set; } = null!;
    public virtual DbSet<WhiteTokkunStageResult> WhiteTokkunStageResults { get; set; } = null!;
    public virtual DbSet<DanScoreDatumWhite> DanScoreDataWhite { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatumWhite> DanStageScoreDataWhite { get; set; } = null!;
    public virtual DbSet<WhiteDonChallengeRawFact> WhiteDonChallengeRawFacts { get; set; } = null!;
    public virtual DbSet<WhiteDonChallengeProgress> WhiteDonChallengeProgress { get; set; } = null!;

    partial void OnModelCreatingWhite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSaveDataWhite>(entity =>
        {
            entity.ToTable("UserSaveData_White");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SongBestDatumWhite>(entity =>
        {
            entity.ToTable("SongBestDatum_White");
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

        modelBuilder.Entity<SongPlayDatumWhite>(entity =>
        {
            entity.ToTable("SongPlayDatum_White");
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

        modelBuilder.Entity<WhiteFavoriteSongs>(entity =>
        {
            entity.ToTable("WhiteFavoriteSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WhiteRecentSongs>(entity =>
        {
            entity.ToTable("WhiteRecentSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.Property(e => e.LastPlayed).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WhiteTokkunStageResult>(entity =>
        {
            entity.ToTable("WhiteTokkunStageResults");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.Baid, e.PlayDatetime });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanScoreDatumWhite>(entity =>
        {
            entity.ToTable("DanScoreDatum_White");
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

        modelBuilder.Entity<DanStageScoreDatumWhite>(entity =>
        {
            entity.ToTable("DanStageScoreDatum_White");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra, e.StageIndex });
            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasForeignKey(d => new { d.Baid, d.DanId, d.IsExtra })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WhiteDonChallengeRawFact>(entity =>
        {
            entity.ToTable("WhiteDonChallengeRawFacts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.BundleId).HasMaxLength(64);
            entity.Property(e => e.PlayTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.HasIndex(e => new { e.Baid, e.BundleId, e.TaskId, e.TrackNo, e.PlayTime });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WhiteDonChallengeProgress>(entity =>
        {
            entity.ToTable("WhiteDonChallengeProgress");
            entity.HasKey(e => new { e.Baid, e.BundleId, e.TaskId, e.TrackNo });
            entity.Property(e => e.BundleId).HasMaxLength(64);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.CompletedAt).HasColumnType("datetime");
            entity.HasIndex(e => new { e.BundleId, e.TaskId, e.TrackNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
