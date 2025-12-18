using Library.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Library.Domain.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<InventoryRecord> InventoryRecords { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\ProjectModels;Database=LibraryDB;Trusted_Connection=True;");
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region DateOnly converter
            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                d => d.ToDateTime(TimeOnly.MinValue),
                dt => DateOnly.FromDateTime(dt)
            );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType == null) continue;

                var dateOnlyProps = clrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateOnly));
                foreach (var prop in dateOnlyProps)
                {
                    modelBuilder.Entity(clrType)
                        .Property(prop.Name)
                        .HasConversion(dateOnlyConverter)
                        .HasColumnType("date");
                }
            }
            #endregion

            #region Relationships

            var today = DateOnly.FromDateTime(DateTime.Now);

            // - Book relationships -
            //Book -> Category (many Books have one Category)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Book -> Author (many Books have one Author)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Book -> InventoryRecord (one Book has many InventoryRecords / copies)
            modelBuilder.Entity<InventoryRecord>()
                .HasOne(ir => ir.Book)
                .WithMany(b => b.InventoryRecords)
                .HasForeignKey(ir => ir.BookId)
                .OnDelete(DeleteBehavior.Restrict);


            // - Publisher relationships -
            // Publisher -> InventoryRecord (one Publisher has many copies)
            modelBuilder.Entity<InventoryRecord>()
                .HasOne(ir => ir.Publisher)
                .WithMany(p => p.InventoryRecords)
                .HasForeignKey(ir => ir.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);


            // - InventoryRecord relationships -
            // InventoryRecord -> BorrowRecord (one copy has many borrow history records)
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.InventoryRecord)
                .WithMany(ir => ir.BorrowRecords)
                .HasForeignKey(br => br.InventoryRecordId)
                .OnDelete(DeleteBehavior.Restrict);


            // - User relationships -
            // User -> BorrowRecord (one User has many BorrowRecords)
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.User)
                .WithMany(u => u.BorrowRecords)
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // - UserType -> User -
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserType)
                .WithMany(ut => ut.Users)
                .HasForeignKey(u => u.UserTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion


            //Globally unique CopyCode
            modelBuilder.Entity<InventoryRecord>()
                .HasIndex(ir => ir.CopyCode)
                .IsUnique();

            //Prevents making more UserType Ids with the same Role
            modelBuilder.Entity<UserType>()
                .HasIndex(ut => ut.Role)
                .IsUnique();


            #region Seeding
            modelBuilder.Entity<Author>().HasData(
                new Author { Id = -1, Name = "Unknown", Email = string.Empty, CreatedDate = today });

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = -1, Name = "Unknown", CreatedDate = today });

            modelBuilder.Entity<Publisher>().HasData
                (new Publisher { Id = -1, Name = "Unknown", CreatedDate = today });

            modelBuilder.Entity<UserType>().HasData(
                new UserType { Id = -1, Role = "Admin", CreatedDate = today },
                new UserType { Id = -2, Role = "Normal", CreatedDate = today }
            );
            #endregion


            //Default new users to Normal role
            modelBuilder.Entity<User>()
                .Property(u => u.UserTypeId)
                .HasDefaultValue(-2);
        }
    }
}
