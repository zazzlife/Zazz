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

        AccountType GetUserAccountType(int userId);

        Gender GetUserGender(int userId);

        Gender GetUserGender(string username);

        string GetUserFullName(int userId);

        string GetUserFullName(string username);

        int GetUserPhotoId(int userId);

        int GetUserPhotoId(string username);

        int GetUserCoverPhotoId(int userId);
        
        string GetUserName(int userId);

        void ResetPhotoId(int photoId);

        bool WantsFbEventsSynced(int userId);

        bool WantsFbPostsSynced(int userId);

        bool WantsFbImagesSynced(int userId);
    }
}