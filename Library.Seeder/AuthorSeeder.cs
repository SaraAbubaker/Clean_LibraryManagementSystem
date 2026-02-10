using Library.Entities.Models;
using Library.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Seeder
{
    public static class AuthorSeeder
    {
        public static async Task SeedAsync(LibraryContext db)
        {
            var authors = new[]
            {
                new Author { Name = "George Orwell" },
                new Author { Name = "Jane Austen" },
                new Author { Name = "J.K. Rowling" },
                new Author { Name = "Mark Twain" },
                new Author { Name = "Harper Lee" },
                new Author { Name = "Suzanne Collins" },
                new Author { Name = "Mary Shelley" },
                new Author { Name = "Traditional" },
                new Author { Name = "Frank Herbert" },
                new Author { Name = "James Clear" },
                new Author { Name = "Chris Colfer" },
                new Author { Name = "F. Scott Fitzgerald" },
                new Author { Name = "J.R.R. Tolkien" },
                new Author { Name = "John Smith" }
            };

            foreach (var author in authors)
            {
                if (!await db.Authors.AnyAsync(a => a.Name == author.Name))
                {
                    db.Authors.Add(author);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
