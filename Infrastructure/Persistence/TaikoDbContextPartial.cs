using TaikoLocalServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TaikoLocalServer.Infrastructure.Persistence;

// The server's own tables, the same for every game: the Web UI's player extras, access, sessions, cabinets.
public partial class TaikoDbContext
{
    public virtual DbSet<UserWebProfile> UserWebProfiles { get; set; } = null!;
    public virtual DbSet<AccessRules> AccessRules { get; set; } = null!;
    public virtual DbSet<ServerSecrets> ServerSecrets { get; set; } = null!;
    public virtual DbSet<ActivityEntry> Activity { get; set; } = null!;
    public virtual DbSet<SignInAttempt> SignInAttempts { get; set; } = null!;
    public virtual DbSet<BannedCard> BannedCards { get; set; } = null!;
    public virtual DbSet<WebSession> WebSessions { get; set; } = null!;
    public virtual DbSet<ApiToken> ApiTokens { get; set; } = null!;
    public virtual DbSet<DashboardNotice> DashboardNotices { get; set; } = null!;
    public virtual DbSet<ServerStatus> ServerStatuses { get; set; } = null!;
    public virtual DbSet<TrustedBrowser> TrustedBrowsers { get; set; } = null!;
    public virtual DbSet<Cabinet> Cabinets { get; set; } = null!;
    public virtual DbSet<MovieLabel> MovieLabels { get; set; } = null!;
    public virtual DbSet<ShopLiveSeason> ShopLiveSeasons { get; set; } = null!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
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
        modelBuilder.Entity<DashboardNotice>(entity => entity.Property(e => e.Id).ValueGeneratedNever());
        modelBuilder.Entity<ServerStatus>(entity => entity.Property(e => e.Id).ValueGeneratedNever());
        modelBuilder.Entity<TrustedBrowser>(entity => entity.HasIndex(e => e.TokenHash).IsUnique());
        modelBuilder.Entity<Cabinet>(entity => entity.HasKey(e => e.ChassisId));
        modelBuilder.Entity<ShopLiveSeason>(entity => entity.HasKey(e => e.Era));
        modelBuilder.Entity<MovieLabel>(entity =>
        {
            entity.HasKey(e => e.MovieId);
            entity.Property(e => e.MovieId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(MovieLabel.MaxLength);
        });

        modelBuilder.Entity<UserWebProfile>(entity =>
        {
            entity.HasKey(e => e.Baid);

            entity.HasOne(d => d.Ba)
                .WithOne()
                .HasForeignKey<UserWebProfile>(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
