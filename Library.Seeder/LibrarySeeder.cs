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

            //call when you want a new copy of for a new book
            //await InventorySeeder.SeedAsync(db); 
        }
    }
}