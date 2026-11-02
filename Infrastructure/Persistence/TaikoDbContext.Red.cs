using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataRed> UserSaveDataRed { get; set; } = null!;
    public virtual DbSet<SongBestDatumRed> SongBestDataRed { get; set; } = null!;
    public virtual DbSet<SongPlayDatumRed> SongPlayDataRed { get; set; } = null!;
    public virtual DbSet<RedFavoriteSongs> RedFavoriteSongs { get; set; } = null!;
    public virtual DbSet<RedRecentSongs> RedRecentSongs { get; set; } = null!;
    public virtual DbSet<DanScoreDatumRed> DanScoreDataRed { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatumRed> DanStageScoreDataRed { get; set; } = null!;
    public virtual DbSet<RedDonChallengeRawFact> RedDonChallengeRawFacts { get; set; } = null!;
    public virtual DbSet<RedDonChallengeProgress> RedDonChallengeProgress { get; set; } = null!;

    partial void OnModelCreatingRed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSaveDataRed>(entity =>
        {
            entity.ToTable("UserSaveData_Red");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SongBestDatumRed>(entity =>
        {
            entity.ToTable("SongBestDatum_Red");
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

        modelBuilder.Entity<SongPlayDatumRed>(entity =>
        {
            entity.ToTable("SongPlayDatum_Red");
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

        modelBuilder.Entity<RedFavoriteSongs>(entity =>
        {
            entity.ToTable("RedFavoriteSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RedRecentSongs>(entity =>
        {
            entity.ToTable("RedRecentSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.Property(e => e.LastPlayed).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanScoreDatumRed>(entity =>
        {
            entity.ToTable("DanScoreDatum_Red");
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

        modelBuilder.Entity<DanStageScoreDatumRed>(entity =>
        {
            entity.ToTable("DanStageScoreDatum_Red");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra, e.StageIndex });
            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasForeignKey(d => new { d.Baid, d.DanId, d.IsExtra })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RedDonChallengeRawFact>(entity =>
        {
            entity.ToTable("RedDonChallengeRawFacts");
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

        modelBuilder.Entity<RedDonChallengeProgress>(entity =>
        {
            entity.ToTable("RedDonChallengeProgress");
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
