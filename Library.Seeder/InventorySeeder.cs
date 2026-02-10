using Library.Domain.Data;
using Library.Entities.Models;
using Library.Shared.Helper;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Library.Seeder
{
    public static class InventorySeeder
    {
        public static async Task SeedAsync(LibraryContext context)
        {
            var books = await context.Books.ToListAsync();
            var random = new Random();

            foreach (var book in books)
            {
                // Random desired total copies (2–6)
                int desiredTotalCopies = random.Next(2, 7);

                // Get existing copies for this book
                var existingCopies = await context.InventoryRecords
                    .Where(i => i.BookId == book.Id)
                    .ToListAsync();

                int existingCount = existingCopies.Count;

                if (existingCount >= desiredTotalCopies)
                    continue;

                // 🔥 Extract highest existing copy number (last 2 digits)
                int maxNumber = 0;

                foreach (var copy in existingCopies)
                {
                    var match = Regex.Match(copy.CopyCode, @"-(\d{2})$");
                    if (match.Success)
                    {
                        int number = int.Parse(match.Groups[1].Value);
                        if (number > maxNumber)
                            maxNumber = number;
                    }
                }

                var newInventory = new List<InventoryRecord>();

                int copiesToCreate = desiredTotalCopies - existingCount;

                for (int i = 0; i < copiesToCreate; i++)
                {
                    maxNumber++;

                    string copyCode = CopyCodeGeneratorHelper
                        .GenerateCopyCode(book.Title, book.Id, maxNumber);

                    newInventory.Add(new InventoryRecord
                    {
                        BookId = book.Id,
                        PublisherId = book.PublisherId,
                        CopyCode = copyCode,
                        IsAvailable = true
                    });
                }

                if (newInventory.Any())
                {
                    await context.InventoryRecords.AddRangeAsync(newInventory);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}