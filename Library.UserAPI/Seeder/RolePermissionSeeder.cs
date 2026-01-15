using Library.UserAPI.Data;
using Library.UserAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Seeder
{
    public static class RolePermissionSeeder
    {
        public static readonly string[] Permissions = new[]
        {
            "user.manage",       // register, login, logout, deactivate, reactivate, archive
            "user.basic",        // register, login, logout (self-service)

            "usertype.manage",   // create, update, archive, view
            "auth.manage",       // jwt, refresh, hash

            "author.manage",     // create, update, archive, view

            "book.manage",       // create, update, archive, search, view
            "book.basic",        // search, view available books

            "borrow.manage",     // borrow, return, availability, overdue
            "borrow.basic",      // borrow, return

            "category.manage",   // create, update, archive, view
            "category.basic",    // view only

            "inventory.manage",  // create copy, return copy, archive copy, view copies

            "publisher.manage",  // create, update, archive, view
            "publisher.basic"    // view only
        };

        // Librarian: operational domains + limited user actions
        private static readonly string[] LibrarianPermissions = new[]
        {
            "user.basic",
            "author.manage",
            "book.manage",
            "borrow.manage",
            "category.manage",
            "inventory.manage",
            "publisher.manage"
        };

        // Customer: self-service + limited view/borrow
        private static readonly string[] CustomerPermissions = new[]
        {
            "user.basic",
            "book.basic",
            "borrow.basic",
            "category.basic",
            "publisher.basic"
        };


        public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager, ApplicationDbContext db)
        {
            // 1. Seed Roles
            var roles = new[] { "Admin", "Librarian", "Customer" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow)
                    });
                }
            }

            // 2. Seed Permissions
            foreach (var perm in Permissions)
            {
                if (!await db.Permissions.AnyAsync(p => p.PermissionName == perm))
                {
                    db.Permissions.Add(new Permission { PermissionName = perm });
                }
            }
            await db.SaveChangesAsync();

            // 3. Assign Permissions to Roles
            await Assign(db, "Admin", Permissions);
            await Assign(db, "Librarian", LibrarianPermissions);
            await Assign(db, "Customer", CustomerPermissions);
        }

        private static async Task Assign(ApplicationDbContext db, string roleName, string[] permissions)
        {
            var role = await db.Roles.FirstAsync(r => r.Name == roleName);

            foreach (var permName in permissions)
            {
                var perm = await db.Permissions.FirstAsync(p => p.PermissionName == permName);

                bool exists = await db.RolePermissions.AnyAsync(rp =>
                    rp.RoleId == role.Id && rp.PermissionId == perm.Id);

                if (!exists)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = perm.Id
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
