using Library.UserAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Data
{
    /*
     * ApplicationDbContext extends IdentityDbContext<ApplicationUser, ApplicationRole, int>
     *
     * IdentityDbContext already provides:
     *   - DbSets for AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, AspNetUserLogins, AspNetRoleClaims
     *   - Built-in EF Core configuration for Identity schema
     */
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // NEW: DbSet for refresh tokens
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // rename tables for cleaner names
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");

            // Default value for IsArchived = false
            builder.Entity<ApplicationUser>()
                .Property(u => u.IsArchived)
                .HasDefaultValue(false);

            builder.Entity<ApplicationRole>()
                .Property(r => r.IsArchived)
                .HasDefaultValue(false);

            // CreatedDate defaults to current date
            builder.Entity<ApplicationUser>()
                .Property(u => u.CreatedDate)
                .HasDefaultValueSql("getdate()");

            builder.Entity<ApplicationRole>()
                .Property(r => r.CreatedDate)
                .HasDefaultValueSql("getdate()");

            // Map RefreshToken entity to "RefreshTokens" table with Id as primary key
            builder.Entity<RefreshToken>().ToTable("RefreshTokens");
            builder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            // One-to-many relationship w/ User
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}