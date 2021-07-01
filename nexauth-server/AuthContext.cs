using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using nexauth_server.Models;

namespace nexauth_server.Models {
    public class AuthContext : DbContext {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            builder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseInMemoryDatabase("AuthContext");
        }

        public DbSet<AuthRequest> AuthRequests { get; set; }

        public DbSet<nexauth_server.Models.User> User { get; set; }
        public DbSet<nexauth_server.Models.AuthRequest> Requests { get; set; }
    }
}
