using EntityFramework.Exceptions.Sqlite;
using TaikoLocalServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.Persistence
{
    public partial class TaikoDbContext : DbContext
    {
        private string? dbFilePath;
        public TaikoDbContext()
        {
        }

        public TaikoDbContext(DbContextOptions<TaikoDbContext> options)
            : base(options)
        {
        }

        public TaikoDbContext(string dbFilePath)
        {
            this.dbFilePath = dbFilePath;
        }

        public virtual DbSet<Card> Cards { get; set; } = null!;
        public virtual DbSet<Credential> Credentials { get; set; } = null!;
        public virtual DbSet<SongBestDatum> SongBestData { get; set; } = null!;
        public virtual DbSet<SongPlayDatum> SongPlayData { get; set; } = null!;
        public virtual DbSet<UserDatum> UserData { get; set; } = null!;
        public virtual DbSet<UserWebProfile> UserWebProfiles { get; set; } = null!;
        public virtual DbSet<UserAvatar> UserAvatars { get; set; } = null!;
        public virtual DbSet<AccessRules> AccessRules { get; set; } = null!;
        public virtual DbSet<ServerSecrets> ServerSecrets { get; set; } = null!;
        public virtual DbSet<ActivityEntry> Activity { get; set; } = null!;
        public virtual DbSet<SignInAttempt> SignInAttempts { get; set; } = null!;
        public virtual DbSet<BannedCard> BannedCards { get; set; } = null!;
        public virtual DbSet<WebSession> WebSessions { get; set; } = null!;
        public virtual DbSet<ApiToken> ApiTokens { get; set; } = null!;
        public virtual DbSet<BackupSchedule> BackupSchedules { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            var path = Path.Combine(PathHelper.GetRootPath(), "taiko.db3");
            if (dbFilePath is not null)
            {
                path = dbFilePath;
            }
            optionsBuilder.UseSqlite($"Data Source={path}");
            optionsBuilder.UseExceptionProcessor().EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.AccessCode);

                entity.ToTable("Card");
                
                entity.HasOne(d => d.Ba)
                    .WithMany()
                    .HasPrincipalKey(p => p.Baid)
                    .HasForeignKey(d => d.Baid)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Single-row tables (Id 1), created by ServerAccess at first start.
            modelBuilder.Entity<AccessRules>(entity => entity.Property(e => e.Id).ValueGeneratedNever());
            modelBuilder.Entity<ServerSecrets>(entity => entity.Property(e => e.Id).ValueGeneratedNever());
            modelBuilder.Entity<ActivityEntry>(entity => entity.HasIndex(e => e.Time));
            modelBuilder.Entity<SignInAttempt>(entity =>
            {
                entity.HasIndex(e => e.Time);
                entity.HasIndex(e => new { e.Baid, e.Time });
            });
            modelBuilder.Entity<BannedCard>(entity => entity.HasKey(e => e.AccessCode));
            modelBuilder.Entity<WebSession>(entity => entity.HasIndex(e => e.Baid));
            modelBuilder.Entity<ApiToken>(entity => entity.HasIndex(e => e.Hash).IsUnique());
            modelBuilder.Entity<BackupSchedule>(entity => entity.Property(e => e.Id).ValueGeneratedNever());

            modelBuilder.Entity<UserWebProfile>(entity =>
            {
                entity.HasKey(e => e.Baid);

                entity.Property(e => e.FriendCode).HasMaxLength(Domain.FriendCode.Length);
                entity.HasIndex(e => e.FriendCode).IsUnique();

                entity.HasOne(d => d.Ba)
                    .WithOne()
                    .HasForeignKey<UserWebProfile>(d => d.Baid)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserAvatar>(entity =>
            {
                entity.HasKey(e => e.Baid);

                entity.Property(e => e.ContentType).HasMaxLength(16);
                entity.Property(e => e.Key).HasMaxLength(UserAvatar.KeyMaxLength);

                entity.HasOne(d => d.Ba)
                    .WithOne()
                    .HasForeignKey<UserAvatar>(d => d.Baid)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Credential>(entity =>
            {
                entity.HasKey(e => e.Baid);

                entity.ToTable("Credential");
                
                entity.HasOne(d => d.Ba)
                    .WithMany()
                    .HasPrincipalKey(p => p.Baid)
                    .HasForeignKey(d => d.Baid)
                    .OnDelete(DeleteBehavior.Cascade);
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

                entity.Property(e => e.AchievementDisplayDifficulty)
                    .HasConversion<uint>();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}