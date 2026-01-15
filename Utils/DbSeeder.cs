using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.DBObjects;
using System.Text;

namespace MiniStrava.Utils
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db, IConfiguration config, ILogger logger)
        {
            // Admin
            var adminEmail = config["Admin:Email"] ?? "admin@ministrava.local";
            var adminTempPassword = config["Admin:TempPassword"] ?? "Admin!12345";

            var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (admin == null)
            {
                CreatePasswordHash(adminTempPassword, out var hash, out var salt);

                admin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = adminEmail,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsAdmin = true,
                    PreferredLanguage = "pl",
                    CreatedAt = DateTimeOffset.UtcNow,
                    MustChangePassword = true
                };

                db.Users.Add(admin);
                await db.SaveChangesAsync();

                logger.LogInformation("Admin created: {Email} (temp password set, must change on first login).", adminEmail);
            }

            // 5 test users
            var testPassword = config["Seed:TestPassword"] ?? "Test!12345";

            for (int i = 1; i <= 5; i++)
            {
                var email = $"test{i}@example.com";
                if (await db.Users.AnyAsync(u => u.Email == email)) continue;

                CreatePasswordHash(testPassword, out var hash, out var salt);

                db.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    FirstName = $"Test{i}",
                    LastName = "User",
                    PreferredLanguage = "pl",
                    CreatedAt = DateTimeOffset.UtcNow,
                    MustChangePassword = false
                });
            }

            await db.SaveChangesAsync();

            // aktywność testowa dla test1 (z trackpointami)
            var test1 = await db.Users.FirstOrDefaultAsync(u => u.Email == "test1@example.com");
            if (test1 != null && !await db.Activities.AnyAsync(a => a.UserId == test1.Id))
            {
                var start = DateTimeOffset.UtcNow.AddDays(-1);

                var act = new Activity
                {
                    Id = Guid.NewGuid(),
                    UserId = test1.Id,
                    Name = "Sample Run",
                    ActivityType = ActivityType.running,
                    StartTime = start,
                    EndTime = start.AddMinutes(25),
                    DurationSeconds = 1500,
                    DistanceMeters = 4200,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                db.Activities.Add(act);

                db.TrackPoints.AddRange(
                    new TrackPoint { ActivityId = act.Id, Sequence = 1, Timestamp = start, Latitude = 51.107885m, Longitude = 17.038538m },
                    new TrackPoint { ActivityId = act.Id, Sequence = 2, Timestamp = start.AddSeconds(10), Latitude = 51.107900m, Longitude = 17.038550m },
                    new TrackPoint { ActivityId = act.Id, Sequence = 3, Timestamp = start.AddSeconds(20), Latitude = 51.107920m, Longitude = 17.038560m }
                );

                await db.SaveChangesAsync();
                logger.LogInformation("Seeded sample activity + trackpoints for test1@example.com");
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // Spójne z LoginService/Register -> PasswordTools
            var saltStr = PasswordTools.GenerateSalt(); // Base64 string
            var hashStr = PasswordTools.GenerateHash(password, saltStr); // Base64 string

            passwordSalt = Encoding.UTF8.GetBytes(saltStr);
            passwordHash = Encoding.UTF8.GetBytes(hashStr);
        }
    }
}
