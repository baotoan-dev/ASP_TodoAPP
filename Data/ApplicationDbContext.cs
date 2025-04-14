using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<TodoContainer> TodoContainers { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fix lỗi MySQL: BLOB/TEXT column used in key specification without a key length
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.Id).HasMaxLength(255);
            });

            builder.Entity<IdentityRole>(b =>
            {
                b.Property(r => r.Id).HasMaxLength(255);
            });

            builder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.Property(l => l.LoginProvider).HasMaxLength(255);
                b.Property(l => l.ProviderKey).HasMaxLength(255);
            });

            builder.Entity<IdentityUserRole<string>>(b =>
            {
                b.Property(r => r.UserId).HasMaxLength(255);
                b.Property(r => r.RoleId).HasMaxLength(255);
            });

            builder.Entity<IdentityUserToken<string>>(b =>
            {
                b.Property(t => t.UserId).HasMaxLength(255);
                b.Property(t => t.LoginProvider).HasMaxLength(255);
                b.Property(t => t.Name).HasMaxLength(255);
            });

            builder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(255);
                b.Property(c => c.UserId).HasMaxLength(255);
            });

            builder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(255);
                b.Property(c => c.RoleId).HasMaxLength(255);
            });
        }
    }
}
