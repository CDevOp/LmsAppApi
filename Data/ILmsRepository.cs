using System.Collections.Generic;
using System.Threading.Tasks;
using LmsApp.API.Models;

namespace LmsApp.API.Data
{
    public interface ILmsRepository
    {
         void Add<T>(T entity) where T: class;
         void Delete<T>(T entity) where T: class;
         Task<bool> SaveAll();
         Task<IEnumerable<User>> GetUsers();
         Task<User> GetUser(int id);
    }
}