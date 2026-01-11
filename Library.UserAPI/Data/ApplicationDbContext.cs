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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            //rename tables for cleaner names
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");


            //Default value for IsArchived = false
            builder.Entity<ApplicationUser>()
                .Property(u => u.IsArchived)
                .HasDefaultValue(false);

            builder.Entity<ApplicationRole>()
                .Property(r => r.IsArchived)
                .HasDefaultValue(false);

            //CreatedDate defaults to current date
            builder.Entity<ApplicationUser>()
                .Property(u => u.CreatedDate)
                .HasDefaultValueSql("getdate()");

            builder.Entity<ApplicationRole>()
                .Property(r => r.CreatedDate)
                .HasDefaultValueSql("getdate()");

        }
    }
}