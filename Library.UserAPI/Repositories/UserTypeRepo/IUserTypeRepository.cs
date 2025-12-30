using Library.UserAPI.Models;

namespace Library.UserAPI.Repositories.UserTypeRepo
{
    public interface IUserTypeRepository
    {
        IQueryable<UserType> GetAll();
        IQueryable<UserType> GetById(int id);
        Task CommitAsync();
    }
}