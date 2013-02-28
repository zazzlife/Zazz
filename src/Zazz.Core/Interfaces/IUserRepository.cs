using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);

        Task<User> GetByUsernameAsync(string username);

        Task<int> GetIdByEmailAsync(string email);

        int GetIdByUsername(string username);
            
        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByUsernameAsync(string username);

        Task<string> GetUserPassword(string username);
    }
}