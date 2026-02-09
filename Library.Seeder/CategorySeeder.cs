using Library.Entities.Models;
using Library.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Seeder
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(LibraryContext db)
        {
            var categories = new[]
            {
                new Category { Name = "Fiction" },
                new Category { Name = "Fantasy" },
                new Category { Name = "Classics" },
                new Category { Name = "Science Fiction" },
                new Category { Name = "Mystery" },
                new Category { Name = "Young Adult" },
                new Category { Name = "Horror" },
                new Category { Name = "Children's" },
                new Category { Name = "Non-Fiction" },
                new Category { Name = "Biography" },
                new Category { Name = "History" },
                new Category { Name = "Romance" },
                new Category { Name = "Adventure" },
                new Category { Name = "Thriller" }
            };

            foreach (var category in categories)
            {
                if (!await db.Categories.AnyAsync(c => c.Name == category.Name))
                {
                    db.Categories.Add(category);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}