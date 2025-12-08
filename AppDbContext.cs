using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.DBObjects;

namespace MiniStrava
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
