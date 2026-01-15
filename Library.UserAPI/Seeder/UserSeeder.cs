using Library.UserAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace Library.UserAPI.Seeder
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            await SeedUserAsync(userManager, "admin", "admin@library.local", "Admin@123", "Admin");
            await SeedUserAsync(userManager, "librarian", "librarian@library.local", "Librarian@123", "Librarian");
            await SeedUserAsync(userManager, "customer", "customer@library.local", "Customer@123", "Customer");
        }

        private static async Task SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string username,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsArchived = false,
                    SecurityStamp = Guid.NewGuid().ToString("D")
                };

                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}