using EntityFramework.Exceptions.Sqlite;
using TaikoLocalServer.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.Persistence
{
    public partial class TaikoDbContext : DbContext, ITaikoDbContext
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
            OnModelCreatingShared(modelBuilder);
            OnModelCreatingNijiiro(modelBuilder);
            OnModelCreatingGreen(modelBuilder);
            OnModelCreatingBlue(modelBuilder);
            OnModelCreatingYellow(modelBuilder);
            OnModelCreatingRed(modelBuilder);
            OnModelCreatingWhite(modelBuilder);
            OnModelCreatingMurasaki(modelBuilder);
            OnModelCreatingKimidori(modelBuilder);
            OnModelCreatingMomoiro(modelBuilder);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingShared(ModelBuilder modelBuilder);
        partial void OnModelCreatingNijiiro(ModelBuilder modelBuilder);
        partial void OnModelCreatingGreen(ModelBuilder modelBuilder);
        partial void OnModelCreatingBlue(ModelBuilder modelBuilder);
        partial void OnModelCreatingYellow(ModelBuilder modelBuilder);
        partial void OnModelCreatingRed(ModelBuilder modelBuilder);
        partial void OnModelCreatingWhite(ModelBuilder modelBuilder);
        partial void OnModelCreatingMurasaki(ModelBuilder modelBuilder);
        partial void OnModelCreatingKimidori(ModelBuilder modelBuilder);
        partial void OnModelCreatingMomoiro(ModelBuilder modelBuilder);
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
