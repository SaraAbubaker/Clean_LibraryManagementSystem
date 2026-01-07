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


            // Seed roles using ApplicationRole
            builder.Entity<ApplicationRole>().HasData(
                    new ApplicationRole { Id = -1, Name = "Admin", NormalizedName = "ADMIN", CreatedDate = today },
                    new ApplicationRole { Id = -2, Name = "Normal", NormalizedName = "NORMAL", CreatedDate = today }
             );

            // Seed default admin user
            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = -1,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@library.local",
                NormalizedEmail = "ADMIN@LIBRARY.LOCAL",
                EmailConfirmed = true,
                CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsArchived = false,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            builder.Entity<ApplicationUser>().HasData(adminUser);

            // Link admin user to Admin role
            builder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int>
                {
                    UserId = -1,
                    RoleId = -1
                }
            );

        }
    }
}