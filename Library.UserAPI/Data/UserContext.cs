using Library.UserAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var today = DateOnly.FromDateTime(DateTime.Now);

            // UserType -> User
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserType)
                .WithMany(ut => ut.Users)
                .HasForeignKey(u => u.UserTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate roles
            modelBuilder.Entity<UserType>()
                .HasIndex(ut => ut.Role)
                .IsUnique();

            // Seeding
            modelBuilder.Entity<UserType>().HasData(
                new UserType { Id = -1, Role = "Admin", CreatedDate = today },
                new UserType { Id = -2, Role = "Normal", CreatedDate = today }
            );

            // Default new users to Normal role
            modelBuilder.Entity<User>()
                .Property(u => u.UserTypeId)
                .HasDefaultValue(-2);
        }
    }
}