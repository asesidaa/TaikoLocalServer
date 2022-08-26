using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Common;
using TaikoLocalServer.Entities;

namespace TaikoLocalServer.Context
{
    public partial class TaikoDbContext : DbContext
    {
        public TaikoDbContext()
        {
        }

        public TaikoDbContext(DbContextOptions<TaikoDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Card> Cards { get; set; } = null!;
        public virtual DbSet<SongBestDatum> SongBestData { get; set; } = null!;
        public virtual DbSet<SongPlayDatum> SongPlayData { get; set; } = null!;
        public virtual DbSet<UserDatum> UserData { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }
            var path = Path.Combine(PathHelper.GetDataPath(), Constants.DB_NAME);
            optionsBuilder.UseSqlite($"Data Source={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.AccessCode);

                entity.ToTable("Card");

                entity.HasIndex(e => e.Baid, "IX_Card_Baid")
                    .IsUnique();
            });

            modelBuilder.Entity<SongBestDatum>(entity =>
            {
                entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty });

                entity.HasOne(d => d.Ba)
                    .WithMany()
                    .HasPrincipalKey(p => p.Baid)
                    .HasForeignKey(d => d.Baid)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Difficulty)
                    .HasConversion<uint>();
                
                entity.Property(e => e.BestCrown)
                    .HasConversion<uint>();
                
                entity.Property(e => e.BestScoreRank)
                    .HasConversion<uint>();
            });

            modelBuilder.Entity<SongPlayDatum>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.PlayTime).HasColumnType("datetime");

                entity.HasOne(d => d.Ba)
                    .WithMany()
                    .HasPrincipalKey(p => p.Baid)
                    .HasForeignKey(d => d.Baid)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(e => e.Difficulty)
                    .HasConversion<uint>();
                
                entity.Property(e => e.ScoreRank)
                    .HasConversion<uint>();
                
                entity.Property(e => e.Crown)
                    .HasConversion<uint>();
            });

            modelBuilder.Entity<UserDatum>(entity =>
            {
                entity.HasKey(e => e.Baid);

                entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");

                entity.HasOne(d => d.Ba)
                    .WithMany()
                    .HasPrincipalKey(p => p.Baid)
                    .HasForeignKey(d => d.Baid)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(e => e.AchievementDisplayDifficulty)
                    .HasConversion<uint>();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
