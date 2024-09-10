using GameDatabase.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameDatabase.Context;

public partial class TaikoDbContext
{
    public virtual DbSet<ChallengeCompeteDatum> ChallengeCompeteData { get; set; } = null;
    public virtual DbSet<ChallengeCompeteParticipantDatum> ChallengeCompeteParticipantData { get; set; } = null;
    public virtual DbSet<ChallengeCompeteSongDatum> ChallengeCompeteSongData { get; set; } = null;
    public virtual DbSet<ChallengeCompeteBestDatum> ChallengeCompeteBestData { get; set; } = null;

    partial void OnModelCreatingChallengeCompete(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChallengeCompeteDatum>(entity =>
        {
            entity.HasKey(e => new { e.CompId });

            entity.Property(e => e.CompeteMode).HasConversion<uint>();
            entity.Property(e => e.Share).HasConversion<uint>();
            entity.Property(e => e.CompeteTarget).HasConversion<uint>();

            entity.Property(e => e.CreateTime).HasColumnType("datetime");
            entity.Property(e => e.ExpireTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<ChallengeCompeteParticipantDatum>(entity =>
        {
            entity.HasKey(e => new { e.CompId, e.Baid });

            entity.HasOne(e => e.UserData)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChallengeCompeteData)
                .WithMany(p => p.Participants)
                .HasPrincipalKey(p => p.CompId)
                .HasForeignKey(d => d.CompId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChallengeCompeteSongDatum>(entity =>
        {
            entity.HasKey(e => new { e.CompId, e.SongId });
            
            entity.HasOne(e => e.ChallengeCompeteData)
                .WithMany(p => p.Songs)
                .HasPrincipalKey(p => p.CompId)
                .HasForeignKey(d => d.CompId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Difficulty).HasConversion<uint>();
        });

        modelBuilder.Entity<ChallengeCompeteBestDatum>(entity =>
        {
            entity.HasKey(e => new { e.CompId, e.Baid, e.SongId });

            entity.HasOne(e => e.UserData)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChallengeCompeteSongData)
                .WithMany(p => p.BestScores)
                .HasPrincipalKey(p => new { p.CompId, p.SongId })
                .HasForeignKey(d => new { d.CompId, d.SongId })
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.Crown).HasConversion<uint>();
            entity.Property(e => e.ScoreRank).HasConversion<uint>();
        });
    }
}
