using Library.Domain.Data;

namespace Library.Seeder
{
    public static class LibrarySeeder
    {
        public static async Task SeedAllAsync(LibraryContext db)
        {
            await AuthorSeeder.SeedAsync(db);
            await CategorySeeder.SeedAsync(db);
            await PublisherSeeder.SeedAsync(db);
            await BookSeeder.SeedAsync(db);
            // optionally: InventoryRecordSeeder.SeedAsync(db);
        }
    }
}