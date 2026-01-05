
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Library.UserAPI.Data
{
    public class UserContextFactory : IDesignTimeDbContextFactory<UserContext>
    {
        public UserContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserContext>();
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\ProjectModels;Database=UserDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new UserContext(optionsBuilder.Options);
        }
    }
}
