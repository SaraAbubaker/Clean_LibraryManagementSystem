using Library.Entities.Models;
using Library.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Seeder
{
    public static class BookSeeder
    {
        public static async Task SeedAsync(LibraryContext db)
        {
            var authorIds = (await db.Authors.ToListAsync()).ToDictionary(a => a.Name, a => a.Id);
            var categoryIds = (await db.Categories.ToListAsync()).ToDictionary(c => c.Name, c => c.Id);
            var publisherIds = (await db.Publishers.ToListAsync()).ToDictionary(p => p.Name, p => p.Id);

            Book CreateBook(string title, DateOnly publishDate, string author, string category, string publisher) =>
                new Book
                {
                    Title = title,
                    PublishDate = publishDate,
                    AuthorId = authorIds[author],
                    CategoryId = categoryIds[category],
                    PublisherId = publisherIds[publisher]
                };

            var books = new List<Book>
            {
                //----------Title--------------------------------------PublishDate------------------Author------------------Category-----------Publisher
                CreateBook("1984",                                     new DateOnly(1949, 6, 8),   "George Orwell",        "Science Fiction", "Penguin Books"),
                CreateBook("Animal Farm",                              new DateOnly(1945, 8, 17),  "George Orwell",        "Classics",        "Penguin Books"),
                CreateBook("Pride and Prejudice",                      new DateOnly(1813, 1, 28),  "Jane Austen",          "Classics",        "Oxford Press"),
                CreateBook("Harry Potter and the Philosopher's Stone", new DateOnly(1997, 6, 26),  "J.K. Rowling",         "Fantasy",         "Bloomsbury"),
                CreateBook("Harry Potter and the Chamber of Secrets",  new DateOnly(1998, 7, 2),   "J.K. Rowling",         "Fantasy",         "Bloomsbury"),
                CreateBook("Adventures of Huckleberry Finn",           new DateOnly(1884, 12, 10), "Mark Twain",           "Classics",        "HarperCollins"),
                CreateBook("To Kill a Mockingbird",                    new DateOnly(1960, 7, 11),  "Harper Lee",           "Fiction",         "Random House"),
                CreateBook("The Hunger Games",                         new DateOnly(2008, 9, 14),  "Suzanne Collins",      "Young Adult",     "Scholastic"),
                CreateBook("Frankenstein",                             new DateOnly(1818, 1, 1),   "Mary Shelley",         "Horror",          "Lackington, Hughes, Harding, Mavor & Jones"),
                CreateBook("Jack and the Beanstalk",                   new DateOnly(1734, 1, 1),   "Traditional",          "Children's",      "Various"),
                CreateBook("Dune",                                     new DateOnly(1965, 8, 1),   "Frank Herbert",        "Science Fiction", "Chilton Books"),
                CreateBook("Atomic Habits",                            new DateOnly(2018, 10, 16), "James Clear",          "Non-Fiction",     "Avery"),
                CreateBook("The Land of Stories: The Wishing Spell",   new DateOnly(2012, 7, 17),  "Chris Colfer",         "Fantasy",         "Little, Brown"),
                CreateBook("The Great Gatsby",                         new DateOnly(1925, 4, 10),  "F. Scott Fitzgerald",  "Classics",        "Charles Scribner's Sons"),
                CreateBook("The Hobbit",                               new DateOnly(1937, 9, 21),  "J.R.R. Tolkien",       "Fantasy",         "Allen & Unwin")

            };

            foreach (var book in books)
            {
                if (!await db.Books.AnyAsync(b => b.Title == book.Title))
                {
                    db.Books.Add(book);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
