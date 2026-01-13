using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.DBObjects;

namespace MiniStrava
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        // DODANE:
        public DbSet<Activity> Activities { get; set; }
        public DbSet<TrackPoint> TrackPoints { get; set; }
        public DbSet<ActivityPhoto> ActivityPhotos { get; set; }
        public DbSet<ApiClient> ApiClients { get; set; }
        public DbSet<SyncSession> SyncSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // enum -> nvarchar(50)
            modelBuilder.Entity<Activity>()
                .Property(a => a.ActivityType)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<SyncSession>()
                .Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // relacje
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrackPoint>()
                .HasOne(tp => tp.Activity)
                .WithMany(a => a.TrackPoints)
                .HasForeignKey(tp => tp.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityPhoto>()
                .HasOne(p => p.Activity)
                .WithMany(a => a.ActivityPhotos)
                .HasForeignKey(p => p.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SyncSession>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApiClient>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // indeksy
            modelBuilder.Entity<Activity>()
                .HasIndex(a => new { a.UserId, a.StartTime })
                .HasDatabaseName("IX_Activities_UserId_StartTime");

            modelBuilder.Entity<Activity>()
                .HasIndex(a => a.ActivityType)
                .HasDatabaseName("IX_Activities_ActivityType");

            // kluczowe dla sync paczkami: brak duplikatów Sequence w ramach Activity
            modelBuilder.Entity<TrackPoint>()
                .HasIndex(tp => new { tp.ActivityId, tp.Sequence })
                .IsUnique()
                .HasDatabaseName("UX_TrackPoints_ActivityId_Sequence");

            modelBuilder.Entity<SyncSession>()
                .HasIndex(s => new { s.UserId, s.StartedAt })
                .HasDatabaseName("IX_SyncSessions_UserId_StartedAt");

            modelBuilder.Entity<ActivityPhoto>()
                .HasIndex(p => p.ActivityId)
                .HasDatabaseName("IX_ActivityPhotos_ActivityId");

            modelBuilder.Entity<ApiClient>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_ApiClients_UserId");
        }
    }
}
