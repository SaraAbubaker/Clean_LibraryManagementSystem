using Library.Common.StringConstants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Library.User.Domain.Data;
using Library.User.Entities;

namespace Library.User.Seeder
{
    public static class RolePermissionSeeder
    {
        public static readonly string[] Permissions = new[]
        {
            PermissionNames.UserManage,       // register, login, logout, deactivate, reactivate, archive
            PermissionNames.UserBasic,        // register, login, logout (self-service)

            PermissionNames.UserTypeManage,   // create, update, archive, view
            PermissionNames.AuthManage,       // jwt, refresh, hash

            PermissionNames.AuthorManage,     // create, update, archive, view

            PermissionNames.BookManage,       // create, update, archive, search, view
            PermissionNames.BookBasic,        // search, view available books

            PermissionNames.BorrowManage,     // borrow, return, availability, overdue
            PermissionNames.BorrowBasic,      // borrow, return

            PermissionNames.CategoryManage,   // create, update, archive, view
            PermissionNames.CategoryBasic,    // view only

            PermissionNames.InventoryManage,  // create copy, return copy, archive copy, view copies

            PermissionNames.PublisherManage,  // create, update, archive, view
            PermissionNames.PublisherBasic    // view only
        };

        // Librarian: operational domains + limited user actions
        private static readonly string[] LibrarianPermissions = new[]
        {
            PermissionNames.UserBasic,
            PermissionNames.AuthorManage,
            PermissionNames.BookManage,
            PermissionNames.BookBasic,
            PermissionNames.BorrowManage,
            PermissionNames.BorrowBasic,
            PermissionNames.CategoryManage,
            PermissionNames.CategoryBasic,
            PermissionNames.InventoryManage,
            PermissionNames.PublisherManage,
            PermissionNames.PublisherBasic
        };

        // Customer: self-service + limited view/borrow
        private static readonly string[] CustomerPermissions = new[]
        {
            PermissionNames.UserBasic,
            PermissionNames.BookBasic,
            PermissionNames.BorrowBasic,
            PermissionNames.CategoryBasic,
            PermissionNames.PublisherBasic
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
