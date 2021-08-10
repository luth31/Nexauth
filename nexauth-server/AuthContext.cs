using Microsoft.EntityFrameworkCore;

namespace Nexauth.Server.Models {
    public class AuthContext : DbContext {
        public AuthContext() { }
        public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            builder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseInMemoryDatabase("AuthContext");
        }

        public DbSet<AuthRequest> AuthRequests { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<AuthRequest> Requests { get; set; }
    }
}
