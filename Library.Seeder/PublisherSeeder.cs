using Library.Entities.Models;
using Library.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Seeder
{
    public static class PublisherSeeder
    {
        public static async Task SeedAsync(LibraryContext db)
        {
            var publishers = new[]
            {
                new Publisher { Name = "Penguin Books" },
                new Publisher { Name = "HarperCollins" },
                new Publisher { Name = "Bloomsbury" },
                new Publisher { Name = "Random House" },
                new Publisher { Name = "Oxford Press" },
                new Publisher { Name = "Scholastic" },
                new Publisher { Name = "Lackington, Hughes, Harding, Mavor & Jones" },
                new Publisher { Name = "Various" },
                new Publisher { Name = "Chilton Books" },
                new Publisher { Name = "Avery" },
                new Publisher { Name = "Little, Brown" },
                new Publisher { Name = "Charles Scribner's Sons" },
                new Publisher { Name = "Allen & Unwin" }
            };

            foreach (var publisher in publishers)
            {
                if (!await db.Publishers.AnyAsync(p => p.Name == publisher.Name))
                {
                    db.Publishers.Add(publisher);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
